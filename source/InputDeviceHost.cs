using SysCol = System.Collections.Generic;

namespace ChaosFramework.Input
{
    public interface InputDeviceHost
    {
        SysCol.IEnumerable<InputDevice> RefreshDeviceList();

        void Update();
    }
}
