namespace ChaosFramework.Input.Axis.Hid
{
    public abstract class HidValueAxis(InputDevice parent, HidAxisInfo hidInfo)
        : ValueAxis(parent)
    {
        public readonly HidAxisInfo hidInfo = hidInfo;

        public override string GetAxisString()
           => hidInfo.GetAxisString();
    }
}
