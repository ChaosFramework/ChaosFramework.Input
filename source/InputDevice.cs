using System.Linq;
using System.Collections;
using SysCol = System.Collections.Generic;

namespace ChaosFramework.Input
{
    public abstract class InputDevice : SysCol.IEnumerable<InputAxis>
    {
        internal protected readonly InputContext parent;

        readonly SysCol.Dictionary<uint, InputAxisCollection> indexToAxis = new SysCol.Dictionary<uint, InputAxisCollection>();
        readonly SysCol.Dictionary<InputAxis, uint> axisToIndex = new SysCol.Dictionary<InputAxis, uint>();

        public virtual string deviceName => GetType().Name;
        public virtual string productName => GetType().Name;

        public InputDevice(InputContext parent)
        {
            this.parent = parent;
        }

        public virtual void Update(bool noop = false)
        {
            foreach (InputAxis axis in this)
                if (axis != null)
                {
                    axis.UpdateInternal(null);
                    if (noop)
                        axis.value = 0;
                }
        }

        public uint GetAxisIndex(InputAxis a)
        {
            uint output;
            if (axisToIndex.TryGetValue(a, out output))
                return output;
            return 0;
        }

        public InputAxisCollection this[uint param]
        {
            get
            {
                InputAxisCollection axis;
                if (!indexToAxis.TryGetValue(param, out axis))
                    return null;
                return axis;
            }
        }

        protected void AddAxis(uint usage, SysCol.IEnumerable<InputAxis> axis)
        {
            InputAxisCollection axisList;
            if (!indexToAxis.TryGetValue(usage, out axisList))
                indexToAxis[usage] = axisList = new InputAxisCollection();

            axisList.Add(axis);
            foreach (InputAxis a in axis)
                axisToIndex[a] = usage;
        }

        protected void AddAxis(uint usage, InputAxis axis)
            => AddAxis(usage, new[] { axis });

        protected void AddEvent(InputEvent e)
            => parent.AddEvent(e);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        SysCol.IEnumerator<InputAxis> SysCol.IEnumerable<InputAxis>.GetEnumerator() => GetEnumerator();
        public SysCol.IEnumerator<InputAxis> GetEnumerator()
            => indexToAxis.Values.SelectMany(Collections.Linq.SelectIdentity).GetEnumerator();

    }
}
