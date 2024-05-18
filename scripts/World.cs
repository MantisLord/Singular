using Godot;
using static Game;
using static AudioManager;

public partial class World : Node3D
{
    private Timer spawnTimer;
    private PackedScene blockScene = GD.Load<PackedScene>("res://scenes/block.tscn");
    private PackedScene houseScene = GD.Load<PackedScene>("res://scenes/flying_object_house.tscn");
    private PackedScene carScene = GD.Load<PackedScene>("res://scenes/car.tscn");
    private PackedScene trashcanScene = GD.Load<PackedScene>("res://scenes/trashcan.tscn");
    private PackedScene toiletScene = GD.Load<PackedScene>("res://scenes/toilet.tscn");
    private PackedScene sofaScene = GD.Load<PackedScene>("res://scenes/flying_object_sofa.tscn");
    private PackedScene lampScene = GD.Load<PackedScene>("res://scenes/flying_object_lamp.tscn");
    private Player player;
    private AnimationObject crater;
    private AnimationObject intro;
    private AnimationObject town;
    private AnimationObject islands;
    private AnimationObject rat;
    private AnimationObject outro;
    private RandomNumberGenerator rand;
    private AudioManager audioMgr;
    private Game game;
    private GpuParticles3D boundaryParticles;
    private ColorRect uiCover;
    private DirectionalLight3D light;
    private WorldEnvironment worldEnv;

    private float spawnHeight = -30;
    private float spawnRadiusAroundPlayer = 5;
    private float spawnRadiusPlayArea = 20;
    private double spawnTime = 0.2f;
    private Vector3 playerStartPos = new(-2, 1, -2);

    private const float RESTART_INTRO_TIME = 64; // 220; cheater mode

    private const string INTRO_ANIM_NAME = "ArmatureAction";
    private const string ISLANDS_ANIM_NAME = "Island RigAction_001";
    private const string TOWN_ANIM_NAME = "ArmatureAction_002";
    private const string CRATER_ANIM_NAME = "Crater RigAction";
    private const string OUTRO_ANIM_NAME = "ArmatureAction";

    private const string HEALTHBAR_ANIM_NAME = "EaseInHP";
    private const string FADEWHITE_ANIM_NAME = "FadeToWhite";
    private const string FADEINMENU_ANIM_NAME = "FadeInStatusAndMenu";

    private const string RAT_IDLE_ANIM_NAME = "Idle";
    private const string RAT_DIE_ANIM_NAME = "Death";

    public override void _Ready()
    {
        audioMgr = GetNode<AudioManager>("/root/AudioManager");
        game = GetNode<Game>("/root/Game");
        game.lookEnabled = false;
        game.movementEnabled = false;
        game.gameOver = false;
        game.won = false;
        spawnTimer = GetNode<Timer>("SpawnTimer");
        spawnTimer.Timeout += () => SpawnObstacle();
        spawnTimer.WaitTime = spawnTime;

        player = GetNode<Player>("Player");
        uiCover = player.GetNode<ColorRect>("UI/ColorRect");
        crater = GetNode<AnimationObject>("Crater");
        intro = GetNode<AnimationObject>("Intro");
        town = GetNode<AnimationObject>("Town");
        islands = GetNode<AnimationObject>("Islands");
        rat = GetNode<AnimationObject>("Rat");
        outro = GetNode<AnimationObject>("Outro");
        rand = new RandomNumberGenerator();
        boundaryParticles = GetNode<GpuParticles3D>("BoundaryParticles");
        light = GetNode<DirectionalLight3D>("DirectionalLight3D");
        worldEnv = GetNode<WorldEnvironment>("WorldEnvironment");

        audioMgr.Play(Audio.Opening, AudioChannel.Ambient);
        intro.PlayAnimation(INTRO_ANIM_NAME);
        islands.PlayAnimation(ISLANDS_ANIM_NAME);
        town.PlayAnimation(TOWN_ANIM_NAME);
    }

    public override void _Process(double delta)
    {
    }

    public void Restart()
    {
        DespawnFlyingObjects();
        intro.Visible = true;
        crater.Visible = true;
        town.Visible = true;
        islands.Visible = true;
        boundaryParticles.Visible = true;
        game.movementEnabled = true;
        game.lookEnabled = true;
        game.gameOver = false;
        game.won = false;
        boundaryParticles.Emitting = true;
        player.statusLabel.Visible = true;
        player.healthProgressBar.Value = 100;
        player.Position = playerStartPos;
        spawnTimer.Stop();
        spawnTimer.WaitTime = spawnTime;
        worldEnv.Environment.BackgroundEnergyMultiplier = 1;
        audioMgr.Stop();
        intro.PlayAnimation(INTRO_ANIM_NAME, RESTART_INTRO_TIME);
        islands.PlayAnimation(ISLANDS_ANIM_NAME, RESTART_INTRO_TIME);
        town.PlayAnimation(TOWN_ANIM_NAME, RESTART_INTRO_TIME);
        crater.PlayAnimation(CRATER_ANIM_NAME, RESTART_INTRO_TIME - 8);
        // game music starts at 52 into intro, so at 64 restart time, it would have been running for 12 seconds (64-52=12)
        audioMgr.Play(Audio.GameMusic, AudioChannel.Music, false, (int)RESTART_INTRO_TIME - 52);
    }

    private void DespawnFlyingObjects()
    {
        foreach (Node spawn in GetChildren())
            if (spawn is FlyingObject)
                spawn.QueueFree();
    }

    private void HideWorld()
    {
        crater.anim.Pause();
        islands.anim.Pause();
        town.anim.Pause();

        Tween getDark = CreateTween();
        //getDark.TweenProperty(light, "light_energy", 0, 10);
        getDark.TweenProperty(worldEnv.Environment, "background_energy_multiplier", 0, 3);
        getDark.Play();

        intro.Visible = false;
        crater.Visible = false;
        town.Visible = false;
        islands.Visible = false;
        boundaryParticles.Visible = false;
        boundaryParticles.Emitting = false;
    }

    public async void EasterEgg()
    {
        if (player.menu.Visible)
            player.ToggleIngameMenu();
        DespawnFlyingObjects();
        HideWorld();
        audioMgr.StopBG();
        spawnTimer.Stop();
        rat.Visible = true;
        player.Position = playerStartPos;
        player.cam.LookAt(rat.GlobalPosition);
        game.movementEnabled = false;
        game.lookEnabled = false;
        game.gameOver = true;
        player.Velocity = Vector3.Zero;

        audioMgr.Play(Audio.Rat, AudioChannel.Music);
        rat.PlayAnimation(RAT_IDLE_ANIM_NAME);
        await ToSignal(GetTree().CreateTimer(3), SceneTreeTimer.SignalName.Timeout);
        rat.PlayAnimation(RAT_DIE_ANIM_NAME);
        await ToSignal(GetTree().CreateTimer(4), SceneTreeTimer.SignalName.Timeout);

        System.Environment.Exit(1);
    }

    public void Died(string colliderName)
    {
        if (player.menu.Visible)
            player.ToggleIngameMenu();
        DespawnFlyingObjects();
        HideWorld();
        audioMgr.StopBG();
        audioMgr.Play(Audio.Failure, AudioChannel.Music);

        spawnTimer.WaitTime = spawnTime / 2;
        player.Velocity = Vector3.Zero;
        player.cam.LookAt(new Vector3(player.Position.X, -100, player.Position.Z));
        player.statusLabel.Text = $"You were killed by a {colliderName}. Would you like to restart?";
        player.anim.Play(FADEINMENU_ANIM_NAME);

        game.movementEnabled = false;
        game.lookEnabled = false;
        player.resumeButton.Visible = false;
        game.gameOver = true;
    }

    // Intro Anim Events
    public void StartGameAnims()
    {
        crater.PlayAnimation(CRATER_ANIM_NAME);
    }
    public void StartGameMusic()
    {
        audioMgr.Play(Audio.GameMusic, AudioChannel.Music);
    }
    public void GainControl()
    {
        game.movementEnabled = true;
        boundaryParticles.Emitting = true;
        player.anim.Play(HEALTHBAR_ANIM_NAME);
    }
    public void StartSpawn()
    {
        spawnTimer.Start();
    }

    // Crater Anim Events
    public void StopSpawn()
    {
        spawnTimer.Stop();
    }
    public async void Won()
    {
        boundaryParticles.Emitting = false;
        game.movementEnabled = false;
        game.lookEnabled = false;
        game.gameOver = true;
        player.anim.Play(FADEWHITE_ANIM_NAME);

        // wait for game music to be done
        await ToSignal(GetTree().CreateTimer(20), SceneTreeTimer.SignalName.Timeout);
        audioMgr.Play(Audio.Closing, AudioChannel.Music);
        player.cam.Fov = 45;
        player.colorRect.Visible = false;
        outro.Visible = true;
        crater.Visible = false;
        outro.PlayAnimation(OUTRO_ANIM_NAME);
        player.GlobalPosition = playerStartPos;
        game.outroPlaying = true;


        // wait until player passes through house
        //var wall = outro.GetNode<StaticBody3D>("House Rig/Skeleton3D/Wall Ext W -col/Wall Ext W/StaticBody3D");
        //var roof = outro.GetNode<StaticBody3D>("House Rig/Skeleton3D/Roof -col/Roof/StaticBody3D");
        //player.AddCollisionExceptionWith(wall);
        //player.AddCollisionExceptionWith(roof);
        await ToSignal(GetTree().CreateTimer(16), SceneTreeTimer.SignalName.Timeout);
        //player.RemoveCollisionExceptionWith(wall);
        //player.AddCollisionExceptionWith(roof);
        player.CollisionLayer = 4;
        player.CollisionMask = 4;
        game.movementEnabled = true;
        game.lookEnabled = true;
        game.outroPlaying = false;
    }

    void SpawnObstacle()
    {
        FlyingObject spawn = null;
        FlyingObjectName chosenObject = (FlyingObjectName)rand.RandiRange(0, 5);
        bool relationToPlayer = rand.RandiRange(0, 1) == 1;

        if (game.gameOver && !game.won)
        {
            chosenObject = FlyingObjectName.Toilet; // toilets only, you lost
            relationToPlayer = true;
        }

        switch (chosenObject)
        {
            case FlyingObjectName.House:
                spawn = houseScene.Instantiate<FlyingObject>();
                break;
            case FlyingObjectName.Car:
                spawn = carScene.Instantiate<FlyingObject>();
                break;
            case FlyingObjectName.Sofa:
                spawn = sofaScene.Instantiate<FlyingObject>();
                break;
            case FlyingObjectName.Lamp:
                spawn = lampScene.Instantiate<FlyingObject>();
                break;
            case FlyingObjectName.Toilet:
                spawn = toiletScene.Instantiate<FlyingObject>();
                break;
            case FlyingObjectName.TrashCan:
                spawn = trashcanScene.Instantiate<FlyingObject>();
                break;
        }

        spawn.Position = GetRandomPosOverFloor(relationToPlayer);
        spawn.RotationDegrees = new Vector3(rand.RandfRange(-180, 180), rand.RandfRange(-180, 180), rand.RandfRange(-180, 180));
        spawn.AngularVelocity = new Vector3(rand.RandfRange(-0.5f, 0.5f), rand.RandfRange(-0.5f, 0.5f), rand.RandfRange(-0.5f, 0.5f));

        AddChild(spawn);
    }

    private Vector3 GetRandomPosOverFloor(bool relationToPlayer)
    {
        float X, Y, Z;
        Y = spawnHeight;
        if (relationToPlayer)
        {
            X = player.Position.X + rand.RandfRange(-spawnRadiusAroundPlayer, spawnRadiusAroundPlayer);
            Z = player.Position.Z + rand.RandfRange(-spawnRadiusAroundPlayer, spawnRadiusAroundPlayer);
        }
        else
        {
            X = rand.RandfRange(-spawnRadiusPlayArea, spawnRadiusPlayArea);
            Z = rand.RandfRange(-spawnRadiusPlayArea, spawnRadiusPlayArea);
        }
        return new(X, Y, Z);
    }
}
