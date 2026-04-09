namespace ChaosFramework.Input
{
    using Axis;

    partial class Keyboard
    {
        public abstract class Key(Keyboard parent, HidUsage key)
            : ButtonAxis(parent)
        {
            public readonly HidUsage hidKey = key;

            public override string GetAxisString()
                => hidKey.ToString();
        }
    }
}
