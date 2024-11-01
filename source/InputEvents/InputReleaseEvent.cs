namespace ChaosFramework.Input.InputEvents
{
    public class InputReleaseEvent<Axis>
        : InputEvent<Axis>
        where Axis : InputAxis
    {
        public override EventType type => EventType.Release;

        public InputReleaseEvent(Axis axis, float oldValue, float newValue)
            : base(axis, oldValue, newValue)
        { }
    }
}
