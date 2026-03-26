using System;

namespace InspectorEvents.Core;

public interface IInspectorEventFilter {
    bool Evaluate();
}

public interface ITypedInspectorEventFilter {
    Type FilteredType { get; }
}
public interface IInspectorEventFilter<TEvent> : ITypedInspectorEventFilter {
    Type ITypedInspectorEventFilter.FilteredType => typeof(TEvent);
    bool Evaluate(in TEvent e);
}