using System.Collections;
using SysCol = System.Collections.Generic;

namespace ChaosFramework.Input
{
    public abstract class InputDevice
        : SysCol.IEnumerable<InputAxis>
    {
        internal protected readonly InputContext parent;

        public InputDevice(InputContext parent)
        {
            this.parent = parent;
        }

        public virtual string deviceName
            => GetType().Name;

        public virtual string productName
            => GetType().Name;

        public virtual void AdvanceFrame()
        {
            foreach (InputAxis axis in this)
                axis?.AdvanceFrameInternal();
        }

        /// <summary>
        ///     Returns whether the device is connected during the current logical frame,
        ///     including the frame it was created, even if <see cref="AdvanceFrame"/> has never been called on this instance.
        /// </summary>
        public abstract bool IsConnected();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        SysCol.IEnumerator<InputAxis> SysCol.IEnumerable<InputAxis>.GetEnumerator() => GetEnumerator();

        /// <summary> Returns all currently known axes of this device in an unchanging order. </summary>
        /// <remarks>
        ///     This shall be the order in which the axes are updated and their events are raised.
        ///     This shall ensure that recorded invocation sequences of <see cref="InputContext.UpdateInputConsumption(bool)"/>
        ///     can be repeated deterministically.
        /// </remarks>
        public abstract SysCol.IEnumerator<InputAxis> GetEnumerator();
    }
}
