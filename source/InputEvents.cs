namespace ChaosFramework.Input
{
    public abstract class InputEvent
    {
        public enum EventType
        {
            ChangeOnly,
            Push,
            Repeat,
            Release,
        }

        public readonly float oldValue, newValue;
        public readonly InputAxis axis;

        public abstract EventType type { get; }

        public bool consumed => axis.consumed;

        protected InputEvent(InputAxis axis, float oldValue, float newValue)
        {
            this.axis = axis;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }
    }

    public abstract class InputEvent<Axis>
        : InputEvent
        where Axis : InputAxis
    {
        public new readonly Axis axis;

        public InputEvent(Axis axis, float oldValue, float newValue)
            : base(axis, oldValue, newValue)
        {
            this.axis = axis;
        }
    }
}
