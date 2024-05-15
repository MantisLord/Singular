using Godot;

public partial class Player : CharacterBody3D
{
    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    private Game game;
    private AudioManager audioMgr;

    private Label fpsLabel;
    private Label spawnsLabel;

    private Label debugLabel1;
    private Label debugLabel2;
    private Label debugLabel3;
    private Label debugLabel4;

    private const float MOUSE_SENSITIVITY = 0.15f;
    private const float MAX_ANGLE_VIEW = 90f;
    private const float MIN_ANGLE_VIEW = -90f;
    private const float SPEED = 15.0f;
    private const float SPEED_ACCEL = 8.0f;
    private const float SPEED_DECCEL = 1.0f;

    private Node3D head;
    private Camera3D cam;

    private Control ingameMenu;
    private AudioStreamPlayer3D impactHeavyAudioStreamPlayer;
    private AudioStreamPlayer3D gruntAudioStreamPlayer;

    private RandomNumberGenerator rand = new();

    public ProgressBar healthProgressBar;

    public override void _Ready()
    {
        game = GetNode<Game>("/root/Game");
        audioMgr = GetNode<AudioManager>("/root/AudioManager");
        fpsLabel = GetNode<Label>("UI/VBoxContainer/FPSLabel");
        spawnsLabel = GetNode<Label>("UI/VBoxContainer/SpawnsLabel");
        debugLabel1 = GetNode<Label>("UI/VBoxContainer/DebugLabel");
        debugLabel2 = GetNode<Label>("UI/VBoxContainer/DebugLabel2");
        debugLabel3 = GetNode<Label>("UI/VBoxContainer/DebugLabel3");
        debugLabel4 = GetNode<Label>("UI/VBoxContainer/DebugLabel4");
        head = GetNode<Node3D>("Head");
        cam = head.GetNode<Camera3D>("Camera3D");
        ingameMenu = GetNode<Control>("IngameMenu");
        healthProgressBar = GetNode<ProgressBar>("UI/VBoxContainer/HBoxContainer/HealthProgressBar");
        impactHeavyAudioStreamPlayer = GetNode<AudioStreamPlayer3D>("ImpactHeavyAudioStreamPlayer3D");
        gruntAudioStreamPlayer = GetNode<AudioStreamPlayer3D>("GruntAudioStreamPlayer3D");

        Input.MouseMode = Input.MouseModeEnum.Captured;
        rand.Randomize();
        base._Ready();
    }

    public void TakeHit(int amount)
    {
        healthProgressBar.Value -= amount;
        audioMgr.Play(impactHeavyAudioStreamPlayer);

        if (rand.RandiRange(0,1) == 1)
            audioMgr.Play(gruntAudioStreamPlayer);

        if (healthProgressBar.Value <= 0)
            game.EndGame();
    }

    public void SetNumSpawns(int spawns)
    {
        spawnsLabel.Text = "NumSpawns: " + spawns;
    }

    public override void _Process(double delta)
    {
        if (game.camLookBone != null && !game.lookEnabled)
        {
            cam.LookAt(game.camLookBone.GlobalPosition);
        }

        if (Input.IsActionJustPressed("escape"))
        {
            ingameMenu.Visible = !ingameMenu.Visible;
            Input.MouseMode = (ingameMenu.Visible ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured);
            if (ingameMenu.Visible)
                Input.WarpMouse(new Vector2(GetViewport().GetVisibleRect().Size.X / 2, GetViewport().GetVisibleRect().Size.Y - 500));
        }

        fpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
        debugLabel1.Text = $"Position: {Position}";
        debugLabel2.Text = $"MoveEnabled: {game.movementEnabled} | LookEnabled: {game.lookEnabled}";
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
