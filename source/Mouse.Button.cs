namespace ChaosFramework.Input
{
    using Axis;

    public partial class Mouse
    {
        public enum ButtonSemantic
        {
            Left,
            Right,
            Middle,
        }

        public abstract class Button(Mouse parent, ButtonSemantic button)
            : ButtonAxis(parent)
        {
            public readonly ButtonSemantic button = button;

            public override string GetAxisString()
                => $"Mouse Button {button}";
        }
    }
}
