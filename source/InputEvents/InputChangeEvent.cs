namespace ChaosFramework.Input.InputEvents
{
    public class InputChangeEvent<Axis>
        : InputEvent<Axis>
        where Axis : InputAxis
    {
        public override EventType type => EventType.ChangeOnly;

        public InputChangeEvent(Axis axis, float oldValue, float newValue)
            : base(axis, oldValue, newValue)
        { }
    }
}
