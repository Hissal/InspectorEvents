using System;
using System.Linq;
using System.Reflection;
using InspectorEvents.Core;
using InspectorEvents.Handlers;
using NUnit.Framework;

namespace InspectorEvents.Tests.Editor {
    public sealed class IEH_TypedNumericTransformCallTests {
        const BindingFlags c_instanceFieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void DedicatedHandlers_ExposeOnlyTheirOwnTypedInterface() {
            AssertHandledType<IEH_IntTransformCall>(typeof(int));
            AssertHandledType<IEH_LongTransformCall>(typeof(long));
            AssertHandledType<IEH_FloatTransformCall>(typeof(float));
            AssertHandledType<IEH_DoubleTransformCall>(typeof(double));
        }

        [Test]
        public void DedicatedHandlers_OnlySerializeTheirOwnFieldSet() {
            AssertSerializedFieldLayout<IEH_IntTransformCall, int>();
            AssertSerializedFieldLayout<IEH_LongTransformCall, long>();
            AssertSerializedFieldLayout<IEH_FloatTransformCall, float>();
            AssertSerializedFieldLayout<IEH_DoubleTransformCall, double>();
        }

        [Test]
        public void IntHandler_AppliesConfiguredTransforms() {
            AssertIntTransform(NumericTransformMode.Set, handler => SetField(handler, "setValue", 12), 3, 12);
            AssertIntTransform(NumericTransformMode.Add, handler => SetField(handler, "addValue", 7), 3, 10);
            AssertIntTransform(NumericTransformMode.Multiply, handler => SetField(handler, "multiplyFactor", -4f), 3, -12);
            AssertIntTransform(NumericTransformMode.ClampRange, handler => {
                SetField(handler, "clampMin", 100);
                SetField(handler, "clampMax", 0);
            }, 150, 100);
        }

        [Test]
        public void LongHandler_AppliesConfiguredTransforms() {
            AssertLongTransform(NumericTransformMode.Set, handler => SetField(handler, "setValue", 42L), 3L, 42L);
            AssertLongTransform(NumericTransformMode.Add, handler => SetField(handler, "addValue", 7L), 3L, 10L);
            AssertLongTransform(NumericTransformMode.Multiply, handler => SetField(handler, "multiplyFactor", -4f), 3L, -12L);
            AssertLongTransform(NumericTransformMode.ClampRange, handler => {
                SetField(handler, "clampMin", 100L);
                SetField(handler, "clampMax", 0L);
            }, 150L, 100L);
        }

        [Test]
        public void FloatHandler_AppliesConfiguredTransforms() {
            AssertFloatTransform(NumericTransformMode.Set, handler => SetField(handler, "setValue", 12.5f), 3f, 12.5f);
            AssertFloatTransform(NumericTransformMode.Add, handler => SetField(handler, "addValue", 7.5f), 3f, 10.5f);
            AssertFloatTransform(NumericTransformMode.Multiply, handler => SetField(handler, "multiplyFactor", -4f), 3f, -12f);
            AssertFloatTransform(NumericTransformMode.ClampRange, handler => {
                SetField(handler, "clampMin", 100f);
                SetField(handler, "clampMax", 0f);
            }, 150f, 100f);
        }

        [Test]
        public void DoubleHandler_AppliesConfiguredTransforms() {
            AssertDoubleTransform(NumericTransformMode.Set, handler => SetField(handler, "setValue", 12.5d), 3d, 12.5d);
            AssertDoubleTransform(NumericTransformMode.Add, handler => SetField(handler, "addValue", 7.5d), 3d, 10.5d);
            AssertDoubleTransform(NumericTransformMode.Multiply, handler => SetField(handler, "multiplyFactor", -4d), 3d, -12d);
            AssertDoubleTransform(NumericTransformMode.ClampRange, handler => {
                SetField(handler, "clampMin", 100d);
                SetField(handler, "clampMax", 0d);
            }, 150d, 100d);
        }

        [Test]
        public void IntHandler_MultiplyAcceptsFractionalFloatFactors_WithMidpointsRoundedAwayFromZero() {
            AssertIntTransform(NumericTransformMode.Multiply, handler => SetField(handler, "multiplyFactor", 1.5f), 3, 5);
            AssertIntTransform(NumericTransformMode.Multiply, handler => SetField(handler, "multiplyFactor", 0.5f), 5, 3);
            AssertIntTransform(NumericTransformMode.Multiply, handler => SetField(handler, "multiplyFactor", 0.5f), -5, -3);
        }

        [Test]
        public void LongHandler_MultiplyAcceptsFractionalFloatFactors_WithMidpointsRoundedAwayFromZero() {
            AssertLongTransform(NumericTransformMode.Multiply, handler => SetField(handler, "multiplyFactor", 1.5f), 3L, 5L);
            AssertLongTransform(NumericTransformMode.Multiply, handler => SetField(handler, "multiplyFactor", 0.5f), 5L, 3L);
            AssertLongTransform(NumericTransformMode.Multiply, handler => SetField(handler, "multiplyFactor", 0.5f), -5L, -3L);
        }

        [Test]
        public void DedicatedHandlers_SummariesMatchTheirTypeSpecificSemantics() {
            var intHandler = new IEH_IntTransformCall();
            SetMode(intHandler, NumericTransformMode.Multiply);
            SetField(intHandler, "multiplyFactor", 1.5f);

            var longHandler = new IEH_LongTransformCall();
            SetMode(longHandler, NumericTransformMode.ClampRange);
            SetField(longHandler, "clampMin", 10L);
            SetField(longHandler, "clampMax", -5L);

            var floatHandler = new IEH_FloatTransformCall();
            SetMode(floatHandler, NumericTransformMode.Multiply);
            SetField(floatHandler, "multiplyFactor", 1.5f);

            var doubleHandler = new IEH_DoubleTransformCall();
            SetMode(doubleHandler, NumericTransformMode.Set);
            SetField(doubleHandler, "setValue", 2.25d);

            Assert.That(intHandler.GetSummary(), Is.EqualTo("Int Transform: x => Round(x * 1.5)"));
            Assert.That(longHandler.GetSummary(), Is.EqualTo("Long Transform: x => Clamp(x, -5, 10)"));
            Assert.That(floatHandler.GetSummary(), Is.EqualTo("Float Transform: x => x * 1.5"));
            Assert.That(doubleHandler.GetSummary(), Is.EqualTo("Double Transform: x => 2.25"));
        }

        [Test]
        public void DedicatedHandlers_ChainTheirOwnTypedHandlers() {
            var firstInt = new IEH_IntTransformCall();
            var secondInt = new IEH_IntTransformCall();
            var intCapture = new CaptureHandler<int>();

            SetMode(firstInt, NumericTransformMode.Add);
            SetField(firstInt, "addValue", 5);
            SetHandlers(firstInt, secondInt);

            SetMode(secondInt, NumericTransformMode.Multiply);
            SetField(secondInt, "multiplyFactor", 2f);
            SetHandlers(secondInt, intCapture);

            ((IInspectorEventHandler<int>)firstInt).Handle(3);

            Assert.That(intCapture.InvocationCount, Is.EqualTo(1));
            Assert.That(intCapture.LastValue, Is.EqualTo(16));

            var firstDouble = new IEH_DoubleTransformCall();
            var secondDouble = new IEH_DoubleTransformCall();
            var doubleCapture = new CaptureHandler<double>();

            SetMode(firstDouble, NumericTransformMode.Add);
            SetField(firstDouble, "addValue", 1.25d);
            SetHandlers(firstDouble, secondDouble);

            SetMode(secondDouble, NumericTransformMode.Multiply);
            SetField(secondDouble, "multiplyFactor", 2d);
            SetHandlers(secondDouble, doubleCapture);

            ((IInspectorEventHandler<double>)firstDouble).Handle(3d);

            Assert.That(doubleCapture.InvocationCount, Is.EqualTo(1));
            Assert.That(doubleCapture.LastValue, Is.EqualTo(8.5d).Within(0.0001d));
        }

        static void AssertHandledType<THandler>(Type expectedHandledType) {
            var handledTypes = typeof(THandler).GetInterfaces()
                .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IInspectorEventHandler<>))
                .Select(type => type.GetGenericArguments()[0])
                .ToArray();

            CollectionAssert.AreEquivalent(new[] { expectedHandledType }, handledTypes);
        }

        static void AssertSerializedFieldLayout<THandler, TEvent>() {
            var fields = typeof(THandler).GetFields(c_instanceFieldFlags);
            var fieldNames = fields.Select(field => field.Name).ToArray();

            CollectionAssert.AreEquivalent(
                new[] { "mode", "setValue", "addValue", "multiplyFactor", "clampMin", "clampMax", "handlers" },
                fieldNames
            );

            var handlersField = fields.Single(field => field.Name == "handlers");
            Assert.That(handlersField.FieldType, Is.EqualTo(typeof(IInspectorEventHandler<TEvent>[])));
        }

        static void AssertIntTransform(NumericTransformMode mode, Action<IEH_IntTransformCall> configure, int input, int expected) {
            var handler = new IEH_IntTransformCall();
            var capture = new CaptureHandler<int>();

            SetMode(handler, mode);
            configure(handler);
            SetHandlers(handler, capture);

            ((IInspectorEventHandler<int>)handler).Handle(input);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.EqualTo(expected));
        }

        static void AssertLongTransform(NumericTransformMode mode, Action<IEH_LongTransformCall> configure, long input, long expected) {
            var handler = new IEH_LongTransformCall();
            var capture = new CaptureHandler<long>();

            SetMode(handler, mode);
            configure(handler);
            SetHandlers(handler, capture);

            ((IInspectorEventHandler<long>)handler).Handle(input);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.EqualTo(expected));
        }

        static void AssertFloatTransform(NumericTransformMode mode, Action<IEH_FloatTransformCall> configure, float input, float expected) {
            var handler = new IEH_FloatTransformCall();
            var capture = new CaptureHandler<float>();

            SetMode(handler, mode);
            configure(handler);
            SetHandlers(handler, capture);

            ((IInspectorEventHandler<float>)handler).Handle(input);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.EqualTo(expected).Within(0.0001f));
        }

        static void AssertDoubleTransform(NumericTransformMode mode, Action<IEH_DoubleTransformCall> configure, double input, double expected) {
            var handler = new IEH_DoubleTransformCall();
            var capture = new CaptureHandler<double>();

            SetMode(handler, mode);
            configure(handler);
            SetHandlers(handler, capture);

            ((IInspectorEventHandler<double>)handler).Handle(input);

            Assert.That(capture.InvocationCount, Is.EqualTo(1));
            Assert.That(capture.LastValue, Is.EqualTo(expected).Within(0.0001d));
        }

        static void SetMode<THandler>(THandler handler, NumericTransformMode mode) {
            SetField(handler, "mode", mode);
        }

        static void SetField<THandler>(THandler handler, string fieldName, object value) {
            GetField(typeof(THandler), fieldName).SetValue(handler, value);
        }

        static void SetHandlers<TEvent>(object handler, params IInspectorEventHandler<TEvent>[] handlers) {
            GetField(handler.GetType(), "handlers").SetValue(handler, handlers);
        }

        static FieldInfo GetField(Type type, string fieldName) {
            return type.GetField(fieldName, c_instanceFieldFlags)
                   ?? throw new InvalidOperationException($"Field '{fieldName}' was not found on '{type.Name}'.");
        }

        sealed class CaptureHandler<T> : IInspectorEventHandler<T> {
            public int InvocationCount { get; private set; }
            public T LastValue { get; private set; } = default!;

            public void Handle(in T value) {
                InvocationCount++;
                LastValue = value;
            }
        }
    }
}
