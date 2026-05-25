namespace ChaosFramework.Input.Axis.Hid
{
    public class HidAxisInfo(ushort usagePage, ushort usageIndex)
    {
        public readonly uint usageAndPage = (uint)(usagePage << 16) | usageIndex;
        public readonly ushort usagePage = usagePage, usageIndex = usageIndex;

        public string GetAxisString()
            => usagePage switch
            {
                0x9 => "Button" + usageIndex.ToString("X2"),
                0xC => "Media" + usageIndex.ToString("X2"),
                _   => usagePage.ToString("X2") + usageIndex.ToString("X2"),
            };
    }
}
