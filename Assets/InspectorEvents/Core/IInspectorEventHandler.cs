using System;

namespace InspectorEvents.Core;

public interface IInspectorEventHandler {
    void Handle();
}

public interface ITypedInspectorEventHandler {
    Type HandledType { get; }
}
public interface IInspectorEventHandler<TEvent> : ITypedInspectorEventHandler {
    Type ITypedInspectorEventHandler.HandledType => typeof(TEvent);
    void Handle(in TEvent e);
}