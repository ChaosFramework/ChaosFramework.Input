using System;

namespace ChaosFramework.Input
{
    public class WeightedInputAxis : InputAxis
    {
        public readonly float weight;
        public readonly InputAxis baseAxis;

        public WeightedInputAxis(InputAxis baseAxis, float weight)
            : base(baseAxis.parent)
        {
            this.weight = weight;
            this.baseAxis = baseAxis;
        }

        public override float value
        {
            get { return baseAxis.value * weight; }
            protected internal set { baseAxis.value = value; }
        }

        public override float ValueExponential(float exponent = 4)
            => (float)Math.Pow(baseAxis.value, exponent) * weight;

        protected override void Update(object data) { }

        public override string GetAxisString() => $"WeightedAxis{{axis={{{baseAxis.GetAxisString()}}}; {{weight={weight}}}}}";
    }
}
