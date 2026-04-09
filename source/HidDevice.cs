namespace ChaosFramework.Input
{
    /// <summary> Any HID input device that is not keyboard or mouse. </summary>
    public abstract class HidDevice(InputContext parent)
        : InputDevice(parent)
    {
        protected abstract InputAxis GetByUsageInternal(HidPage hidPage, ushort hidUsage, int subIndex);
    }
}
