using System;
using System.Collections.Generic;
using System.Linq;
using InspectorEvents.Physics3D;
using NUnit.Framework;

namespace InspectorEvents.Tests.Editor.Physics3D {
    public sealed class InspectorPhysics3DEventsTests {
        [Test]
        public void Callbacks_UseValidFlags() {
            AssertValidFlags(InspectorPhysics3DCallbacks.None, InspectorPhysics3DCallbacks.All);
        }

        static void AssertValidFlags<TEnum>(TEnum none, TEnum all) where TEnum : struct, Enum {
            Assert.That(Convert.ToInt32(none), Is.EqualTo(0));

            var values = Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Where(value => !EqualityComparer<TEnum>.Default.Equals(value, none) && !EqualityComparer<TEnum>.Default.Equals(value, all))
                .ToArray();

            foreach (var callback in values) {
                var intValue = Convert.ToInt32(callback);
                Assert.That(intValue > 0 && (intValue & (intValue - 1)) == 0, Is.True, $"{callback} must be a single bit.");
            }

            Assert.That(Convert.ToInt32(all), Is.EqualTo(values.Aggregate(0, (current, value) => current | Convert.ToInt32(value))));
        }
    }
}
