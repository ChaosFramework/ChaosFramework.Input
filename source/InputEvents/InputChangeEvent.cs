namespace ChaosFramework.Input.InputEvents
{
    public class InputChangeEvent<Axis>(Axis axis, InputChange change)
        : InputEvent<Axis, InputChange>(axis, change)
        where Axis : InputAxis
        ;
}
