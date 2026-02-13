namespace ChaosFramework.Input.InputEvents
{
    public class InputPushEvent<Axis>(Axis axis, InputChange change)
        : InputEvent<Axis, InputChange>(axis, change)
        where Axis : InputAxis
        ;
}
