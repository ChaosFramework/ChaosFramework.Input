using System.Collections.Generic;

namespace ChaosFramework.Input
{
    public interface InputDeviceHost
        : IEnumerable<InputDevice>
    {
        void RefreshDeviceList();

        void Update();
    }
}
