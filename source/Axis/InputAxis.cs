namespace ChaosFramework.Input
{
    public abstract class InputAxis(InputDevice parent)
    {
        public readonly InputDevice parent = parent;

        // TODO: remove
        public object customData;

        public virtual float value { get; }

        protected InputContext context
            => parent.parent;

        internal void AdvanceFrameInternal()
            => AdvanceFrame();

        protected abstract void AdvanceFrame();

        public abstract string GetAxisString();

        protected internal void AddEvent(InputEvent e)
            => context.AddEvent(e);
    }
}
