namespace ChaosFramework.Input
{

    /// <summary> Any HID input device that is not known by the framework implementation. </summary>
    public abstract class OtherHidDevice(InputContext parent)
        : HidDevice(parent)
    {
        public InputAxis GetByUsage(HidPage hidPage, ushort hidUsage, int subIndex)
            => GetByUsageInternal(hidPage, hidUsage, subIndex);
    }
}
