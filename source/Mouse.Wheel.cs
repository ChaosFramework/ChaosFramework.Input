namespace ChaosFramework.Input
{
    using Axis;

    public partial class Mouse
    {
        public enum WheelDirection
        {
            Scroll,
            Tilt
        }

        public abstract class Wheel(Mouse parent, WheelDirection dir)
            : ValueAxis(parent)
        {
            public readonly WheelDirection dir = dir;

            public override string GetAxisString()
                => $"Mouse Wheel {dir}";
        }
    }
}
