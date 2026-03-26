using System;
using UnityEngine;

namespace InspectorEvents {
   public struct ExampleEvent : ISerializedEvent {
      public string message;
   }
   
   public readonly struct TestEvent : ISerializedEvent {
      readonly int value;
      readonly float otherValue;

      public TestEvent(int value, float otherValue) {
         this.value = value;
         this.otherValue = otherValue;
      }
   }
   
   public readonly struct TestEvent2 : ISerializedEvent {
      readonly int value;
      readonly float otherValue;

      public Vector3 myVector { get; init; }
      
      public TestEvent2(int value, float otherValue) {
         this.value = value;
         this.otherValue = otherValue;
         myVector = Vector3.zero;
      }

      public override string ToString() {
         return $"TestEvent2: value={value}, otherValue={otherValue}, myVector={myVector}";
      }
   }
   
   public interface ISerializedEvent { }

   public class EventListenerBehavior : EventListenerBehaviorBase<ISerializedEvent> {
      protected override void OnSubscribe<TEvent>(Action<TEvent> call) {
         // TODO: subscribe to event bus or similar with the provided call
         Debug.Log("Subscribed to event: " + typeof(TEvent).Name);
      }

      public override void Unsubscribe() {
         // TODO: unsubscribe from event bus or similar
         Debug.Log("Unsubscribed from event");
      }
   }
}
