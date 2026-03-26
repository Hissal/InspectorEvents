namespace InspectorEvents.Core;

public interface ISerializedEventListener { }

public interface ISerializedEventListener<TEvent> : ISerializedEventListener {
    void OnEvent(in TEvent e);
}