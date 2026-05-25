namespace ChaosFramework.Input.Axis
{
    using InputEvents;

    public abstract class ButtonAxis(InputDevice parent)
        : InputAxis(parent)
    {
        bool intermediate, consistent;

        public override float value => consistent ? 1 : 0;
        public bool down => consistent;

        protected override sealed void AdvanceFrame()
            => consistent = intermediate;

        /// <summary> Updates the intermediate down value and raises respective events on change. </summary>
        protected void SetDown<Self>(bool down)
            where Self : ButtonAxis
        {
            bool tmp = intermediate;
            intermediate = down;
            if (intermediate != tmp)
            {
                InputChange change = new InputChange(tmp ? 1 : 0, intermediate ? 1 : 0);
                AddEvent(intermediate
                    ? (InputEvent)new InputPushEvent<Self>((Self)this, change)
                    : new InputReleaseEvent<Self>((Self)this, change)
                    );
                AddEvent(new InputChangeEvent<Self>((Self)this, change));
            }
        }
    }
}
