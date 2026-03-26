using InspectorEvents.Core;
using UnityEngine;

namespace InspectorEvents {
    public class INspectorEventSerialization : MonoBehaviour {
        public InspectorEvent inspectorEvent;
        public InspectorEvent<int> typedInspectorEvent;
    }
}