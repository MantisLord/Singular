using Godot;
using System;

public partial class Player : CharacterBody3D
{
    public const float Speed = 5.0f;
    public const float JumpVelocity = 4.5f;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    private Label velocityLabel;
    private Label fpsLabel;
    private Label spawnsLabel;

    private const float MOUSE_SENSITIVITY = 0.15f;
    private const float MAX_ANGLE_VIEW = 60f;
    private const float MIN_ANGLE_VIEW = -40f;

    private Node3D head;
    private Camera3D cam;

    public override void _Ready()
    {
        velocityLabel = GetNode<Label>("Control/VBoxContainer/VelocityLabel");
        fpsLabel = GetNode<Label>("Control/VBoxContainer/FPSLabel");
        spawnsLabel = GetNode<Label>("Control/VBoxContainer/SpawnsLabel");
        head = GetNode<Node3D>("Head");
        cam = head.GetNode<Camera3D>("Camera3D");
        Input.MouseMode = Input.MouseModeEnum.Captured;
        base._Ready();
    }

    public void SetNumSpawns(int spawns)
    {
        spawnsLabel.Text = "NumSpawns: " + spawns;
    }

    public override void _Process(double delta)
    {
        fpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
        base._Process(delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.GetType() == typeof(InputEventMouseMotion) && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            InputEventMouseMotion inputEventMouseMotion = (InputEventMouseMotion)@event;
            head.RotateY(Mathf.DegToRad(-inputEventMouseMotion.Relative.X * MOUSE_SENSITIVITY));
            cam.RotateX(Mathf.DegToRad(-inputEventMouseMotion.Relative.Y * MOUSE_SENSITIVITY));
            cam.Rotation = new Vector3(Mathf.Clamp(cam.Rotation.X, Mathf.DegToRad(MIN_ANGLE_VIEW), Mathf.DegToRad(MAX_ANGLE_VIEW)), 0f, 0f);
        }
        base._UnhandledInput(@event);
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        // Add the gravity.
        if (!IsOnFloor())
            velocity.Y -= gravity * (float)delta;

        // Handle Jump.
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
            velocity.Y = JumpVelocity;

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
        }

        Velocity = velocity;
        velocityLabel.Text = velocity.ToString();
        MoveAndSlide();
    }
}
