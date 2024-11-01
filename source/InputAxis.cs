using ChaosFramework.Collections;
using System;

namespace ChaosFramework.Input
{
    public abstract class InputAxis
    {
        internal static float SelectValue(InputAxis axis) => axis.value;
        internal static bool WasReleased(InputAxis axis, float threshold) => axis.WasReleased(threshold);
        internal static bool WasActivated(InputAxis axis, float threshold) => axis.WasActivated(threshold);

        public readonly InputDevice parent;
        public float pushThreshold = 0.5f;
        public object customData;

        public bool consumed { get; internal set; } = false;
        public float oldValue { get; private set; }
        public virtual float value { get; protected internal set; }

        protected InputContext context => parent.parent;

        LinkedList<Action<object>> _onUpdate;
        public LinkedList<Action<object>> onUpdate => _onUpdate ?? (_onUpdate = new LinkedList<Action<object>>());

        public InputAxis(InputDevice parent)
        {
            this.parent = parent;
        }

        /// <summary> Returns whether this axis exceeded a given threshold during the last frame. </summary>
        /// <param name="threshold"> Axis is considered pressed when its value is greater or equal to <paramref name="threshold"/>. </param>
        public bool WasActivated(float threshold = 0.5f) => oldValue < threshold && value >= threshold;

        /// <summary> Returns whether this axis fell below a given threshold during the last frame. </summary>
        /// <param name="threshold"> Axis is considered released when its value is lower than <paramref name="threshold"/>. </param>
        public bool WasReleased(float threshold = 0.5f) => oldValue >= threshold && value < threshold;

        public virtual float ValueExponential(float exponent = 4) => (float)Math.Pow(value, exponent);

        internal void UpdateInternal(object data)
        {
            oldValue = value;
            Update(data);

            if (_onUpdate != null)
                foreach (Action<object> update in _onUpdate)
                    update(data);
        }

        protected abstract void Update(object data);

        public abstract string GetAxisString();

        public int GetSubAxisIndex()
        {
            uint baseIndex = parent.GetAxisIndex(this);
            InputAxisCollection myCollection = parent[baseIndex];
            return myCollection.IndexOf(this);
        }

        protected internal void AddEvent(InputEvent e) => context.AddEvent(e);
    }
}
