namespace ChaosFramework.Input.Axis.Hid
{
    public abstract class HidBoundedValueAxis(InputDevice parent, HidAxisInfo hidInfo)
        : BoundedValueAxis(parent)
    {
        public readonly HidAxisInfo hidInfo = hidInfo;

        public override string GetAxisString()
           => hidInfo.GetAxisString();
    }
}
