using ChaosFramework.Collections;

namespace ChaosFramework.Input
{
    using ActivationContext = (Keyboard.HidUsage usage, float threshold);

    partial class Keyboard
    {
        public static bool WasActivated(InputContext input, HidUsage usage, float threshold = 0.5f)
            => input.EnumerateDevices<Keyboard>().Any((usage, threshold), WasKeyActivated);

        static bool WasKeyActivated(Keyboard keyboard, ActivationContext keyAndThreshold)
            => keyboard[keyAndThreshold.usage].WasActivated(keyAndThreshold.threshold);

        public static bool WasReleased(InputContext input, HidUsage usage, float threshold = 0.5f)
            => input.EnumerateDevices<Keyboard>().Any((usage, threshold), WasKeyReleased);

        static bool WasKeyReleased(Keyboard keyboard, ActivationContext keyAndThreshold)
            => keyboard[keyAndThreshold.usage].WasActivated(keyAndThreshold.threshold);

        public abstract class Key(Keyboard parent, HidUsage key)
            : InputAxis(parent)
        {
            public readonly HidUsage key = key;

            public override string GetAxisString()
                => key.ToString();
        }

        public static float GetValue(InputContext input, HidUsage usage)
        {
            float value = 0;
            foreach (Keyboard keyboard in input.EnumerateDevices<Keyboard>())
                value = System.Math.Max(value, keyboard[usage].value);

            return value;
        }
    }
}
