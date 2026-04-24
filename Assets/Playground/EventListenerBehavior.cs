using System;
using InspectorEvents.Core;
using UnityEngine;

namespace InspectorEvents {

   public class ExampleEventClass : ISerializedEvent {
      public string message;
   }
   [Serializable]
   public struct TestEvent : ISerializedEvent {
      public int publicValue;
      [SerializeField] float serializedValue;

      public TestEvent(int publicValue, float otherValue) {
         this.publicValue = publicValue;
         this.serializedValue = otherValue;
      }
      
      public override string ToString() {
         return $"TestEvent: publicValue={publicValue}, serializedValue={serializedValue}";
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
   
   public readonly struct TestEvent3 : ISerializedEvent {
      readonly int value;
      readonly TestEvent testEvent;
      readonly ExampleEventClass classValue;

      public TestEvent3(int value, TestEvent testEvent, ExampleEventClass classValue) {
         this.value = value;
         this.testEvent = testEvent;
         this.classValue = classValue;
      }

      // public TestEvent3(int value, ExampleEventClass classValue) {
      //    this.value = value;
      //    this.testEvent = default;
      //    this.classValue = classValue;
      // }
      
      public override string ToString() {
         return $"TestEvent3: value={value}, testEvent={testEvent}, classValue={classValue?.message ?? "null"}";
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
