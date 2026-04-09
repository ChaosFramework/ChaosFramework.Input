using SysCol = System.Collections.Generic;

namespace ChaosFramework.Input
{
    public abstract partial class Keyboard
        : InputDevice
    {
        public Key this[HidUsage key]
            => keys.TryGetValue(key, out Key result) ? result : keys[key] = GenerateKey(key);

        SysCol.Dictionary<HidUsage, Key> keys = [];

        public Keyboard(InputContext parent)
            : base(parent)
        {
            foreach (HidUsage keyCode in new SysCol.HashSet<HidUsage>(ChaosUtil.Reflection.Enum<HidUsage>.GetValues()))
                keys[keyCode] = GenerateKey(keyCode);
        }

        protected abstract Key GenerateKey(HidUsage hidUsage);

        public override sealed SysCol.IEnumerator<InputAxis> GetEnumerator()
            => keys.Values.GetEnumerator();
    }
}
