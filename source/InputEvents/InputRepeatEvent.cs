namespace ChaosFramework.Input.InputEvents
{
    public class InputRepeatEvent<Axis>
        : InputPushEvent<Axis>
        where Axis : InputAxis
    {
        public readonly int repetition;

        public override EventType type => EventType.Repeat;

        public InputRepeatEvent(Axis axis, float oldValue, float newValue, int repetition)
            : base(axis, oldValue, newValue)
        {
            this.repetition = repetition;
        }
    }
}
