namespace ChaosFramework.Input.Axis.Hid
{
    public abstract class HidButtonAxis : ButtonAxis
    {
        public readonly HidAxisInfo hidInfo;
        public HidButtonAxis(InputDevice parent, HidAxisInfo hidInfo)
            : base(parent)
        {
            this.hidInfo = hidInfo;
        }

        public override string GetAxisString()
           => hidInfo.GetAxisString();
    }
}
