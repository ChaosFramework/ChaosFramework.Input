using SysCol = System.Collections.Generic;
using ChaosFramework.Collections.Immutable;

namespace ChaosFramework.Input
{
    public abstract partial class Mouse
        : InputDevice
    {
        public readonly Position x, y;
        public readonly ImmutableArray<Button> buttons;

        public Mouse(InputContext parent)
            : base(parent)
        {
            x = GenerateAxis(Direction.X);
            y = GenerateAxis(Direction.Y);
            buttons = GenerateButtons();
        }

        protected abstract Position GenerateAxis(Direction direction);
        protected abstract ImmutableArray<Button> GenerateButtons();

        public override sealed SysCol.IEnumerator<InputAxis> GetEnumerator()
        {
            yield return x;
            yield return y;
            foreach (Button b in buttons)
                yield return b;
        }
    }
}
