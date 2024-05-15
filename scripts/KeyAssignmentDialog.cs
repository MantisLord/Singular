using Godot;

public partial class KeyAssignmentDialog : ConfirmationDialog
{
    public InputEvent lastInputEvent;
    MainMenu menu;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible)
            return;
        if (IsRecordableInput(@event))
            RecordInputEvent(@event);
    }

    public override async void _Input(InputEvent @event)
    {
        if (!Visible)
            return;
        if (IsButtonReleased(@event))
        {
            if (IsPossibleButtonTrigger(@event))
            {
                await ToSignal(GetTree().CreateTimer(0.05f), SceneTreeTimer.SignalName.Timeout);
            }
            RecordInputEvent(@event);
        }
        base._Input(@event);
    }

    private void RecordInputEvent(InputEvent @event)
    {
        var inputEventText = menu.GetText(@event);
        if (string.IsNullOrEmpty(inputEventText))
            return;
        lastInputEvent = @event;
        DialogText = inputEventText;
        GetOkButton().Disabled = false;
    }

    private bool IsPossibleButtonTrigger(InputEvent @event)
    {
        return (@event is InputEventMouseButton inputMouseButton && inputMouseButton.ButtonIndex == MouseButton.Left) || (@event is InputEventJoypadButton inputJoyButton && inputJoyButton.ButtonIndex == JoyButton.A);
    }
    private bool IsButtonReleased(InputEvent @event)
    {
        return (@event is InputEventMouseButton || @event is InputEventJoypadButton) && @event.IsReleased();
    }
    private bool IsRecordableInput(InputEvent @event)
    {
        return (@event is InputEventKey || @event is InputEventMouseButton || @event is InputEventJoypadButton || (@event is InputEventJoypadMotion joypadMotionEvent && Mathf.Abs(joypadMotionEvent.AxisValue) > 0.5));
    }

    public override void _Ready()
    {
        menu = GetTree().Root.GetNode<MainMenu>("MainMenu");
    }

    public override void _Process(double delta)
    {
    }
}
