namespace ChaosFramework.Input
{
    using Axis;

    public partial class Mouse
    {
        public enum Direction
            : byte
        {
            X,
            Y,
        }

        public abstract class Position(InputDevice parent, Direction direction)
            : ValueAxis(parent)
        {
            public readonly Direction direction = direction;

            public override string GetAxisString()
                => $"Mouse Axis {direction}";
        }
    }
}
