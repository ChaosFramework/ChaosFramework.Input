using SysCol = System.Collections.Generic;

namespace ChaosFramework.Input
{
    public abstract partial class Keyboard
        : InputDevice
    {
        public const HidPage HID_PAGE = HidPage.Keyboard;

        SysCol.Dictionary<HidUsage, Key> pressed = new SysCol.Dictionary<HidUsage, Key>();

        public InputAxis this[HidUsage key] => this[(uint)key].first;

        public Keyboard(InputContext parent)
            : base(parent)
        {
            foreach (HidUsage keyCode in new SysCol.HashSet<HidUsage>(ChaosUtil.Reflection.Enum<HidUsage>.GetValues()))
            {
                Key key = GenerateKey(keyCode);
                AddAxis((uint)keyCode, key);
                pressed[keyCode] = key;
            }
        }

        protected abstract Key GenerateKey(HidUsage hidUsage);
    }
}
