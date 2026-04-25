# InspectorEvents — Codebase Overview

**Package:** `com.hissal.inspectorevents`  
**Unity:** 6000.0+  
**Dependencies:** Odin Inspector, `com.hissal.unity-type-serializer`

## What it is

InspectorEvents is a Unity library for defining event-driven behavior entirely in the Inspector. Instead of wiring events in code, a developer adds an `InspectorEvent` or `InspectorEvent<T>` field to a MonoBehaviour, then configures filters and handlers via the Odin-powered Inspector UI. The serialized configuration is what drives runtime behavior.

## Core concepts

**InspectorEvent / InspectorEvent\<T\>**  
An event container with an array of filters and an array of handlers. Calling `.Invoke()` runs all filters first; if every filter passes, all handlers execute. The generic variant passes a typed payload `T` through the chain.

**Filters (`IInspectorEventFilter` / `IInspectorEventFilter<T>`)**  
Boolean guards on event execution. Evaluated in order; first failure short-circuits. Implementations: `IEF_Throttle`, `IEF_Chance`, `IEF_CallCount`, `IEF_Adapter`.

**Handlers (`IInspectorEventHandler` / `IInspectorEventHandler<T>`)**  
Actions that run when all filters pass. Can be simple (Debug.Log, UnityEvent) or composite (nested filter+handler chains, async delay, numeric transforms). Implementations: `IEH_Debug`, `IEH_UnityEvent`, `IEH_DelayCall`, `IEH_FilterCall`, `IEH_NumericTransformCall`, `IEH_Adapter`, and per-type numeric transforms.

**Generic vs non-generic duality**  
Every core interface and most implementations exist in both non-generic form (for events with no payload) and generic form (for typed payloads). Adapters bridge the two where needed.

**EventListenerBehaviorBase\<T\>**  
A MonoBehaviour base class that uses reflection to subscribe an `InspectorEvent<T>` to a runtime event source by type, selected in the Inspector. Handles Unity lifecycle events via `EventListenerLifecycleHandler`.

**Editor integration**  
Odin custom drawers render events as foldouts with Invoke and Configure buttons. `ValueConstructor` (Editor-only) builds arbitrary typed values from Inspector fields for Editor-time event invocation.

---

## Folder map

| Folder | Purpose |
|--------|---------|
| `Assets/InspectorEvents/Core` | Interfaces, InspectorEvent, EventListener base classes |
| `Assets/InspectorEvents/Handlers` | All `IEH_*` handler implementations |
| `Assets/InspectorEvents/Filters` | All `IEF_*` filter implementations |
| `Assets/InspectorEvents/Internal` | Editor-only utilities (ValueConstructor, EventHandlerSentinel) |
| `Assets/InspectorEvents/Editor` | Odin Inspector custom drawers |
| `Assets/InspectorEvents/Tests.Editor` | Editor unit tests |

## Execution pipeline

```
InspectorEvent.Invoke(value?)
  └─ foreach filter → Evaluate() → false? return
  └─ foreach handler → Handle(value?)
```

Short-circuits on first filter failure. Handlers only run if all filters pass.

## Key types

| Class | Role | Path |
|-------|------|------|
| `InspectorEvent` / `InspectorEvent<T>` | Event container (filters + handlers) | `Core/InspectorEvent.cs` |
| `IInspectorEventFilter` / `<T>` | Filter interface (`bool Evaluate`) | `Core/IInspectorEventFilter.cs` |
| `IInspectorEventHandler` / `<T>` | Handler interface (`void Handle`) | `Core/IInspectorEventHandler.cs` |
| `EventListenerBehaviorBase<T>` | MonoBehaviour base; reflection-based event subscription | `Core/EventListenerBehaviorBase.cs` |
| `EventListenerLifecycleHandler` | Intercepts Unity lifecycle callbacks (Start, OnDestroy, etc.) | `Core/EventListenerLifecycleHandler.cs` |
| `EventHandlerSentinel<T>` | Editor-time wrapper; creates typed `Action<T>` for invocation | `Internal/EventHandlerSentinel.cs` |
| `ValueConstructor` | Editor-only; builds arbitrary typed values from Inspector fields | `Internal/ValueConstructor.cs` |

## Handler inventory (`IEH_*`)

| Class | Behavior |
|-------|----------|
| `IEH_Debug` / `<T>` | Debug.Log / LogWarning / LogError / Break (DEBUG-only) |
| `IEH_UnityEvent` / `<T>` | Invokes a serialized UnityEvent |
| `IEH_DelayCall` / `<T>` | Async delay (Awaitable), then runs nested handlers |
| `IEH_FilterCall` / `<T>` | Nested filter+handler chain (composite) |
| `IEH_NumericTransformCall` | Multi-type numeric transform (int/long/float/double); Set/Add/Multiply/ClampRange modes; chains to typed handlers |
| `IEH_IntTransformCall` / Long / Float / Double | Per-type numeric transform handlers |
| `IEH_Adapter` / `<T>` | Adapts generic handler to non-generic (or reverse) |

## Filter inventory (`IEF_*`)

| Class | Behavior |
|-------|----------|
| `IEF_Throttle<T>` | Rate-limit by time interval (scaled / unscaled / realtime) |
| `IEF_Chance<T>` | Random pass with configurable probability |
| `IEF_CallCount<T>` | Pass on Nth call, after/until threshold, or pass/block cycles |
| `IEF_Adapter` | Adapts non-generic filter for use in generic context |

## Design patterns

- **Pipeline** — Filters evaluate sequentially, then handlers execute sequentially
- **Composite** — `IEH_FilterCall` and `IEH_DelayCall` embed their own filter+handler arrays, enabling arbitrary nesting
- **Adapter** — `IEH_Adapter` and `IEF_Adapter` bridge generic ↔ non-generic interfaces
- **Reflection caching** — `EventListenerBehaviorBase` caches reflection-derived subscription invokers to avoid per-frame overhead

## Assembly structure

| Assembly | Platforms | Purpose |
|----------|-----------|---------|
| `InspectorEvents` | All | Runtime classes, interfaces, handlers, filters |
| `InspectorEvents.Editor` | Editor only | Odin drawers, ValueConstructor, EventHandlerSentinel |
| `InspectorEvents.Editor.Tests` | Editor only | Unit tests |
