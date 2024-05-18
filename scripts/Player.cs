using Godot;

public partial class Player : CharacterBody3D
{
    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    private Game game;
    private AudioManager audioMgr;

    private Label fpsLabel;

    private const float MOUSE_SENSITIVITY = 0.15f;
    private const float MAX_ANGLE_VIEW = 90f;
    private const float MIN_ANGLE_VIEW = -90f;
    private const float SPEED = 13.0f;
    private const float SPEED_ACCEL = 7.0f;
    private const float SPEED_DECCEL = 0.01f;

    private Node3D head;
    private Camera3D cam;

    private AudioStreamPlayer3D gruntAudioStreamPlayer;
    private World world;

    private RandomNumberGenerator rand = new();

    public Button resumeButton;
    public Control menu;
    public Label statusLabel;
    public ProgressBar healthProgressBar;
    public AnimationPlayer anim;

    public override void _Ready()
    {
        game = GetNode<Game>("/root/Game");
        audioMgr = GetNode<AudioManager>("/root/AudioManager");
        fpsLabel = GetNode<Label>("UI/FPSLabel");
        head = GetNode<Node3D>("Head");
        cam = head.GetNode<Camera3D>("Camera3D");
        menu = GetNode<PanelContainer>("UI/MenuContainer");
        healthProgressBar = GetNode<ProgressBar>("UI/MarginContainer/VBoxContainer/HealthProgressBar");
        resumeButton = menu.GetNode<Button>("VBoxContainer/ResumeButton");
        gruntAudioStreamPlayer = GetNode<AudioStreamPlayer3D>("GruntAudioStreamPlayer3D");
        anim = GetNode<AnimationPlayer>("AnimationPlayer");
        world = GetTree().Root.GetNode<World>("World");

        anim.Play("FovAdjust");

        Input.MouseMode = Input.MouseModeEnum.Captured;
        rand.Randomize();
        base._Ready();
    }

    public void TakeHit(int amount, AudioStreamPlayer3D collisionAudio)
    {
        healthProgressBar.Value -= amount;

        audioMgr.Play(collisionAudio);

        if (rand.RandiRange(0, 1) == 1)
            audioMgr.Play(gruntAudioStreamPlayer);

        if (healthProgressBar.Value <= 0)
        {
            world.Died();
        }
    }

    public void ToggleIngameMenu()
    {
        menu.Visible = !menu.Visible;
        Input.MouseMode = (menu.Visible ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured);
        if (menu.Visible)
            Input.WarpMouse(new Vector2(GetViewport().GetVisibleRect().Size.X / 2, GetViewport().GetVisibleRect().Size.Y - 500));
    }

    public override void _Process(double delta)
    {
        if (game.camLookBone != null && !game.lookEnabled)
        {
            cam.LookAt(game.camLookBone.GlobalPosition);
        }

        if (Input.IsActionJustPressed("escape"))
        {
            ToggleIngameMenu();
        }

        fpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
        base._Process(delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (game.lookEnabled)
        {
            if (@event.GetType() == typeof(InputEventMouseMotion) && Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                InputEventMouseMotion inputEventMouseMotion = (InputEventMouseMotion)@event;
                head.RotateY(Mathf.DegToRad(-inputEventMouseMotion.Relative.X * MOUSE_SENSITIVITY));
                cam.RotateX(Mathf.DegToRad(-inputEventMouseMotion.Relative.Y * MOUSE_SENSITIVITY));
                cam.Rotation = new Vector3(Mathf.Clamp(cam.Rotation.X, Mathf.DegToRad(MIN_ANGLE_VIEW), Mathf.DegToRad(MAX_ANGLE_VIEW)), 0f, 0f);
            }
        }
        base._UnhandledInput(@event);
    }

    public override void _PhysicsProcess(double delta)
    {
        float time = (float)delta;
        Vector3 velocity = Velocity;
        Vector3 direction = new();
        Basis aim = cam.GlobalTransform.Basis;

        if (game.movementEnabled)
        {
            if (Input.IsActionPressed("forward"))
                direction -= aim.Z;
            if (Input.IsActionPressed("backward"))
                direction += aim.Z;
            if (Input.IsActionPressed("left"))
                direction -= aim.X;
            if (Input.IsActionPressed("right"))
                direction += aim.X;
        }

        if (!IsOnFloor())
        {
            velocity.Y -= gravity * time;
        }

        if (direction != Vector3.Zero)
        {
            velocity.X = Mathf.Lerp(Velocity.X, direction.X * SPEED, time * SPEED_ACCEL);
            velocity.Z = Mathf.Lerp(Velocity.Z, direction.Z * SPEED, time * SPEED_ACCEL);
        }
        else
        {
            velocity.X = Mathf.Lerp(Velocity.X, 0, time * SPEED_DECCEL);
            velocity.Z = Mathf.Lerp(Velocity.Z, 0, time * SPEED_DECCEL);
        }
        Velocity = velocity;
        MoveAndSlide();
    }
}
