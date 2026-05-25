namespace ChaosFramework.Input.Axis
{
    /// <summary> A value axis whose values are always in [0; 1]. </summary>
    public abstract class BoundedValueAxis(InputDevice parent)
        : ValueAxis(parent)
        ;
}
