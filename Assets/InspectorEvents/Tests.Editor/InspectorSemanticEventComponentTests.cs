using System;
using System.Collections.Generic;
using System.Linq;
using InspectorEvents.Core;
using NUnit.Framework;

namespace InspectorEvents.Tests.Editor {
    public sealed class InspectorSemanticEventComponentTests {
        [Test]
        public void CoreCallbackEnums_UseValidFlags() {
            AssertValidFlags(InspectorGameObjectLifecycleCallbacks.None, InspectorGameObjectLifecycleCallbacks.All);
            AssertValidFlags(InspectorApplicationCallbacks.None, InspectorApplicationCallbacks.All);
            AssertValidFlags(InspectorTickCallbacks.None, InspectorTickCallbacks.All);
            AssertValidFlags(InspectorTransformCallbacks.None, InspectorTransformCallbacks.All);
            AssertValidFlags(InspectorVisibilityCallbacks.None, InspectorVisibilityCallbacks.All);
            AssertValidFlags(InspectorObjectMouseCallbacks.None, InspectorObjectMouseCallbacks.All);
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

            var allFromSingles = values.Aggregate(0, (current, value) => current | Convert.ToInt32(value));
            Assert.That(Convert.ToInt32(all), Is.EqualTo(allFromSingles));
        }
    }
}
