namespace ChaosFramework.Input.Axis
{
    using InputEvents;

    public abstract class ValueAxis(InputDevice parent)
        : InputAxis(parent)
    {
        float intermediate, consistent;

        public override float value => consistent;

        // TODO: consider removing this in favor of consumers consuming events
        public float oldValue { get; private set; }

        protected override void AdvanceFrame()
        {
            oldValue = consistent;
            consistent = intermediate;
        }

        /// <summary> Updates the intermediate value and collects a <see cref="InputChangeEvent{Axis}"/> on change. </summary>
        protected void SetValue<Self>(float newValue)
            where Self : ValueAxis
        {
            // TODO: Perform the bounds assertion in the correct class, e.g. via making this method virtual and overriding in respecting #if DEBUG statements
            System.Diagnostics.Debug.Assert(this is not BoundedValueAxis || (newValue >= 0 && newValue <= 1));
            float tmp = intermediate;
            intermediate = newValue;
            if (intermediate != tmp)
                AddEvent(new InputChangeEvent<Self>((Self)this, new InputChange(tmp, intermediate)));
        }
    }
}
