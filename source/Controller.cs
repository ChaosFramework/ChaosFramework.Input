using SysCol = System.Collections.Generic;

namespace ChaosFramework.Input
{
    using Axis;
    public abstract class Controller(InputContext parent)
        : HidDevice(parent)
    {
        /// <summary> HID usage indices on <see cref="HidPage.Button"/> </summary>
        public enum Buttons
            : ushort
        {
            A = 1,
            B = 2,
            X = 3,
            Y = 4,
            LB = 5,
            RB = 6,
            Back = 7,
            Start = 8,
            LS = 9,
            RS = 10,
            Home = 11,
            Extra = 12,
        }

        /// <summary> HID usage indices on <see cref="HidPage.ControllerStick"/> </summary>
        public enum StickAxis
            : ushort
        {
            X1 = 48,
            Y1 = 49,
            X2 = 51,
            Y2 = 52,
        }

        /// <summary> HID usage indices on <see cref="HidPage.ControllerTrigger"/> </summary>
        public enum TriggerAxis
            : ushort
        {
            TriggerPair1 = 50,
        }

        /// <summary>
        ///     A pair of axes around an origin point that produces directional input,
        ///     such as controller sticks, joysticks, D-Pads or hat switches.
        ///     <para>
        ///         The values of the axes describe the points on or inside the shape (usually a circle, but not bounded to one)
        ///         that the physical hardware can assume.
        ///     </para>
        /// </summary>
        public abstract class AxisArea(AxisArea.AxisFactory create) // TODO: move out of Controller class
        {
            protected delegate BoundedValueAxis AxisFactory(bool x, bool positive);

            public readonly BoundedValueAxis xPositive = create(true, true);
            public readonly BoundedValueAxis xNegative = create(true, false);
            public readonly BoundedValueAxis yPositive = create(false, true);
            public readonly BoundedValueAxis yNegative = create(false, false);

            public SysCol.IEnumerable<BoundedValueAxis> EnumerateAxes()
            {
                yield return xPositive;
                yield return xNegative;
                yield return yPositive;
                yield return yNegative;
            }
        }

        public ButtonAxis GetButtonBySemantic(Buttons button)
            => (ButtonAxis)GetByUsageInternal(HidPage.Button, (ushort)button, 0);

        public BoundedValueAxis GetStickAxisBySemantic(StickAxis axis, bool positive)
            => (BoundedValueAxis)GetByUsageInternal(HidPage.ControllerStick, (ushort)axis, positive ? 0 : 1);

        public BoundedValueAxis GetTriggerBySemantic(TriggerAxis axis, bool left)
            => (BoundedValueAxis)GetByUsageInternal(HidPage.ControllerTrigger, (ushort)axis, left ? 0 : 1);
    }
}
