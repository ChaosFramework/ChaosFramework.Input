using ChaosUtil.Reflection;
using SysCol = System.Collections.Generic;

namespace ChaosFramework.Input
{
    public class InputManager<EnumType>
        where EnumType : struct
    {
        class InputValue
        {
            public float previous, current;

            public InputValue(float previous, float current)
            {
                this.previous = previous;
                this.current = current;
            }
        }

        readonly InputContext input;
        readonly float deviceListUpdateInterval;
        readonly SysCol.Dictionary<EnumType, InputValue> inputState = new SysCol.Dictionary<EnumType, InputValue>();
        readonly SysCol.Dictionary<System.Type, SysCol.Dictionary<EnumType, uint>> inputMapping;

        float deviceListUpdateTimer;

        public InputManager(
            InputContext input,
            SysCol.Dictionary<System.Type, SysCol.Dictionary<EnumType, uint>> inputMapping,
            float deviceListUpdateInterval = 3
            )
        {
            this.input = input;
            if (!typeof(EnumType).IsEnum)
                throw new System.ArgumentException($"{nameof(EnumType)} must be an enum type.");

            this.deviceListUpdateInterval = deviceListUpdateTimer = deviceListUpdateInterval;
            this.inputMapping = inputMapping;
            input.UpdateDeviceList();

            foreach (InputDevice dev in input.EnumerateDevices())
                dev.Update();

            foreach (EnumType e in Enum<EnumType>.GetValues())
                inputState[e] = new InputValue(0, EvaluateInput(e));
        }

        public float GetInput(EnumType axis) => GetState(axis).current;
        public float GetPrevInput(EnumType axis) => GetState(axis).previous;

        public bool WasActivated(EnumType axis, float threshold = 0.5f)
        {
            InputValue v = GetState(axis);
            return v.current >= threshold && v.previous < threshold;
        }

        public bool WasReleased(EnumType axis, float threshold = 0.5f)
        {
            InputValue v = GetState(axis);
            return v.current < threshold && v.previous >= threshold;
        }

        public void AdvanceFrame(float time)
        {
            if ((deviceListUpdateTimer -= time) < 0)
            {
                deviceListUpdateTimer += deviceListUpdateInterval;
                input.UpdateDeviceList();
            }

            foreach (InputDevice device in input.EnumerateDevices())
                device.Update();

            foreach (EnumType input in Enum<EnumType>.GetValues())
            {
                inputState[input].previous = inputState[input].current;
                inputState[input].current = EvaluateInput(input);
            }
        }

        float EvaluateInput(EnumType axis)
        {
            float value = 0;
            foreach (InputDevice inputDevice in input.EnumerateDevices())
            {
                SysCol.Dictionary<EnumType, uint> inputMapping;
                if (this.inputMapping.TryGetValue(inputDevice.GetType(), out inputMapping))
                {
                    uint inputAxisIndex;
                    if (inputMapping.TryGetValue(axis, out inputAxisIndex))
                    {
                        InputAxis inputAxis = inputDevice[inputAxisIndex].first;
                        if (inputAxis != null)
                            value = System.Math.Max(value, inputAxis.value);
                    }
                }
            }

            return value;
        }

        InputValue GetState(EnumType axis)
        {
            InputValue value;
            if (inputState.TryGetValue(axis, out value))
                return value;

            throw new System.Exception("This is no valid input axis.");
        }
    }
}
