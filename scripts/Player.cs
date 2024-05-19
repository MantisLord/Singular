using Godot;

public partial class Player : CharacterBody3D
{
    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    private const float MOUSE_SENSITIVITY = 0.15f;
    private const float MAX_ANGLE_VIEW = 90f;
    private const float MIN_ANGLE_VIEW = -90f;
    
    public float speed = 8.0f;
    public float speedAccel = 1f;
    public float speedDeccel = 0.02f;

    private Game game;
    private AudioManager audioMgr;
    private Label exitLabel;
    private Node3D head;
    private AudioStreamPlayer3D gruntAudioStreamPlayer;
    private World world;
    private RandomNumberGenerator rand = new();
    private RayCast3D interact;

    public Button resumeButton;
    public Control menu;
    public ProgressBar healthProgressBar;
    public AnimationPlayer anim;
    public AnimationPlayer anim2;
    public Camera3D cam;
    public ColorRect colorRect;

    public override void _Ready()
    {
        game = GetNode<Game>("/root/Game");
        audioMgr = GetNode<AudioManager>("/root/AudioManager");
        exitLabel = GetNode<Label>("UI/ExitLabel");
        head = GetNode<Node3D>("Head");
        cam = head.GetNode<Camera3D>("Camera3D");
        menu = GetNode<PanelContainer>("UI/MenuContainer");
        healthProgressBar = GetNode<ProgressBar>("UI/MarginContainer/VBoxContainer/HealthProgressBar");
        resumeButton = menu.GetNode<Button>("VBoxContainer/ResumeButton");
        gruntAudioStreamPlayer = GetNode<AudioStreamPlayer3D>("GruntAudioStreamPlayer3D");
        anim = GetNode<AnimationPlayer>("AnimationPlayer");
        anim2 = GetNode<AnimationPlayer>("AnimationPlayer2");
        world = GetTree().Root.GetNode<World>("World");
        colorRect = GetNode<ColorRect>("UI/ColorRect");
        interact = head.GetNode<RayCast3D>("InteractRay");

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

        if (Input.IsActionJustPressed("escape") && !game.won)
        {
            ToggleIngameMenu();
        }

        if (interact.IsColliding())
        {
            exitLabel.Visible = true;
            if (Input.IsActionJustPressed("interact"))
            {
                colorRect.Visible = true;
                game.movementEnabled = false;
                game.lookEnabled = false;
                Velocity = new(0, 0, 0);
                anim.Play("EndingFlash");
            }
        }
        else
            exitLabel.Visible = false;

        base._Process(delta);
    }

    public void WonGame()
    {
        game.ChangeScene("main_menu");
        Input.MouseMode = Input.MouseModeEnum.Visible;
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
            velocity.X = Mathf.Lerp(Velocity.X, direction.X * speed, time * speedAccel);
            velocity.Z = Mathf.Lerp(Velocity.Z, direction.Z * speed, time * speedAccel);
        }
        else
        {
            velocity.X = Mathf.Lerp(Velocity.X, 0, time * speedDeccel);
            velocity.Z = Mathf.Lerp(Velocity.Z, 0, time * speedDeccel);
        }
        Velocity = velocity;
        MoveAndSlide();
    }
}
