namespace InspectorEvents;

public interface ISerializedEventFilter { }

public interface ISerializedEventFilter<TEvent> : ISerializedEventFilter {
    bool Filter(in TEvent e);
}