using ChaosAnalyzers.ClassIntegrity;

namespace ChaosFramework.Input
{
    public struct InputChange(float oldValue, float newValue)
    {
        public readonly float oldValue = oldValue;
        public readonly float newValue = newValue;
    }

    public abstract class InputEvent
    {
        internal abstract object dataInternal {get;}
        internal abstract InputAxis axisInternal {get;}

        public bool consumed => axisInternal.consumed;

        private protected InputEvent() {}
    }

    [method: ExplicitConstructor(applyToAbstractClasses: false)]
    public abstract class InputEvent<Axis, Data>(Axis axis, Data data)
        : InputEvent
        where Axis : InputAxis
    {
        public readonly Axis axis = axis;
        public readonly Data data = data;

        internal override sealed InputAxis axisInternal => axis;
        internal override sealed object dataInternal => data;
    }
}
