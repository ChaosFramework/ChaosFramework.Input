using ChaosAnalyzers.ClassIntegrity;

namespace ChaosFramework.Input
{
    public readonly struct InputChange(float oldValue, float newValue)
    {
        public readonly float oldValue = oldValue;
        public readonly float newValue = newValue;

        /// <summary> Returns whether this axis exceeded a given threshold during the change. </summary>
        /// <param name="threshold"> Axis is considered pressed when its value is greater or equal to <paramref name="threshold"/>. </param>
        public readonly bool WasActivated(float threshold = 0.5f)
            => oldValue < threshold && newValue >= threshold;

        /// <summary> Returns whether this axis fell below a given threshold during the change. </summary>
        /// <param name="threshold"> Axis is considered released when its value is lower than <paramref name="threshold"/>. </param>
        public readonly bool WasReleased(float threshold = 0.5f)
            => oldValue >= threshold && newValue < threshold;
    }

    public abstract class InputEvent
    {
        internal abstract object dataInternal {get;}
        internal abstract InputAxis axisInternal {get;}

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
