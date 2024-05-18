using Godot;

public partial class Player : CharacterBody3D
{
    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    private const float MOUSE_SENSITIVITY = 0.15f;
    private const float MAX_ANGLE_VIEW = 90f;
    private const float MIN_ANGLE_VIEW = -90f;
    private const float SPEED = 8.0f;
    private const float SPEED_ACCEL = 1f;
    private const float SPEED_DECCEL = 0.02f;

    private Game game;
    private AudioManager audioMgr;
    private Label fpsLabel;
    private Label debugLabel;
    private Node3D head;
    private AudioStreamPlayer3D gruntAudioStreamPlayer;
    private World world;
    private RandomNumberGenerator rand = new();

    public Button resumeButton;
    public Control menu;
    public Label statusLabel;
    public ProgressBar healthProgressBar;
    public AnimationPlayer anim;
    public Camera3D cam;
    public ColorRect colorRect;

    public override void _Ready()
    {
        game = GetNode<Game>("/root/Game");
        audioMgr = GetNode<AudioManager>("/root/AudioManager");
        fpsLabel = GetNode<Label>("UI/FPSLabel");
        debugLabel = GetNode<Label>("UI/DebugLabel");
        statusLabel = GetNode<Label>("UI/MarginContainer/VBoxContainer/StatusLabel");
        head = GetNode<Node3D>("Head");
        cam = head.GetNode<Camera3D>("Camera3D");
        menu = GetNode<PanelContainer>("UI/MenuContainer");
        healthProgressBar = GetNode<ProgressBar>("UI/MarginContainer/VBoxContainer/HealthProgressBar");
        resumeButton = menu.GetNode<Button>("VBoxContainer/ResumeButton");
        gruntAudioStreamPlayer = GetNode<AudioStreamPlayer3D>("GruntAudioStreamPlayer3D");
        anim = GetNode<AnimationPlayer>("AnimationPlayer");
        world = GetTree().Root.GetNode<World>("World");
        colorRect = GetNode<ColorRect>("UI/ColorRect");

        anim.Play("FovAdjust");

        Input.MouseMode = Input.MouseModeEnum.Captured;
        rand.Randomize();
        base._Ready();
    }

    public void TakeHit(int amount, AudioStreamPlayer3D collisionAudio, string colliderName)
    {
        if (game.gameOver)
            return;

        healthProgressBar.Value -= amount;

        audioMgr.Play(collisionAudio);

        if (rand.RandiRange(0, 1) == 1)
            audioMgr.Play(gruntAudioStreamPlayer);

        if (healthProgressBar.Value <= 0)
        {
            world.Died(colliderName);
        }
    }

    public void ToggleIngameMenu()
    {
        menu.Visible = !menu.Visible;
        ToggleMouseControl();
    }

    public void ToggleMouseControl()
    {
        Input.MouseMode = (menu.Visible ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured);
        if (menu.Visible)
            Input.WarpMouse(new Vector2(GetViewport().GetVisibleRect().Size.X / 2, GetViewport().GetVisibleRect().Size.Y - 500));
    }

    public override void _Process(double delta)
    {
        if (game.gameOver)
        {
            if (game.camLookBoneOutro != null && game.outroPlaying)
                cam.LookAt(game.camLookBoneOutro.GlobalPosition);
            return;
        }

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

        if (game.movementEnabled)
        {
            var inputDir = Input.GetVector("backward","forward","left","right");
            velocity.X = inputDir.X;
            velocity.Z = inputDir.Y;
            direction = (head.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
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
