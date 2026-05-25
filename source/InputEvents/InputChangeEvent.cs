namespace ChaosFramework.Input.InputEvents
{
    public class InputChangeEvent<Axis>(Axis axis, InputChange change)
        : InputEvent<Axis, InputChange>(axis, change)
        where Axis : InputAxis
    {
        bool crossed;

        public void MakePush()
            => CrossThreshold(new InputPushEvent<Axis>(axis, data));

        public void MakeRelease()
            => CrossThreshold(new InputReleaseEvent<Axis>(axis, data));

        void CrossThreshold(InputEvent evt)
        {
            if (crossed)
                throw new System.InvalidOperationException();
            else
            {
                crossed = true;
                axis.AddEvent(evt);
            }
        }
    }
}
