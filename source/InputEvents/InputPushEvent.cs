namespace ChaosFramework.Input.InputEvents
{
    public class InputPushEvent<Axis>
        : InputEvent<Axis>
        where Axis : InputAxis
    {
        public override EventType type => EventType.Push;

        public InputPushEvent(Axis axis, float oldValue, float newValue)
            : base(axis, oldValue, newValue)
        { }
    }
}
