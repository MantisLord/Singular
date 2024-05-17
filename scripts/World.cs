using Godot;
using System.Linq;
using static AudioManager;

public partial class World : Node3D
{
    private Timer spawnTimer;
    private Timer houseTimer;
    private Timer holeExpandTimer;
    private PackedScene blockScene = GD.Load<PackedScene>("res://scenes/block.tscn");
    private PackedScene houseScene = GD.Load<PackedScene>("res://scenes/flying_object_house.tscn");
    private PackedScene carScene = GD.Load<PackedScene>("res://scenes/car.tscn");
    private PackedScene trashcanScene = GD.Load<PackedScene>("res://scenes/trashcan.tscn");
    private PackedScene toiletScene = GD.Load<PackedScene>("res://scenes/toilet.tscn");
    private PackedScene sofaScene = GD.Load<PackedScene>("res://scenes/flying_object_sofa.tscn");
    private Player player;
    private AnimationObject crater;
    private AnimationObject intro;
    private AnimationObject town;
    private AnimationObject islands;
    private RandomNumberGenerator rand;
    private AudioManager audioMgr;
    private Game game;
    private GpuParticles3D boundaryParticles;

    private const float SPAWN_HEIGHT = -30;
    private const float SPAWN_RADIUS_AROUND_PLAYER = 5;
    private const float SPAWN_RADIUS_PLAY_AREA = 20;
    public override void _Ready()
    {
        audioMgr = GetNode<AudioManager>("/root/AudioManager");
        game = GetNode<Game>("/root/Game");
        game.gameOver = false;
        game.lookEnabled = false;
        game.movementEnabled = false;
        spawnTimer = GetNode<Timer>("SpawnTimer");
        spawnTimer.Timeout += () => SpawnObstacle();
        player = GetNode<Player>("Player");
        crater = GetNode<AnimationObject>("Crater");
        intro = GetNode<AnimationObject>("Intro");
        town = GetNode<AnimationObject>("Town");
        islands = GetNode<AnimationObject>("Islands");
        rand = new RandomNumberGenerator();
        boundaryParticles = GetNode<GpuParticles3D>("BoundaryParticles");

        audioMgr.Play(Audio.Opening, AudioChannel.Ambient);
        intro.PlayAnimation("ArmatureAction");
        islands.PlayAnimation("Island RigAction_001");
        town.PlayAnimation("ArmatureAction_002");
        crater.PlayAnimation("Crater RigAction");
    }

    public override void _Process(double delta)
    {
    }

    public void Restart()
    {
        foreach (Node spawn in GetChildren())
        {
            if (spawn is FlyingObject obj)
                obj.QueueFree();
        }
        game.gameOver = false;
        game.movementEnabled = true;
        boundaryParticles.Emitting = true;
        game.lookEnabled = true;
        player.healthProgressBar.Value = 100;
        player.statusLabel.Visible = false;
        audioMgr.Stop();
        intro.PlayAnimation("ArmatureAction", 57);
        crater.PlayAnimation("Crater RigAction", 57);
        islands.PlayAnimation("Island RigAction_001", 57);
        town.PlayAnimation("ArmatureAction_002", 57);
        audioMgr.Play(Audio.GameMusic, AudioChannel.Music, false, 16);
    }

    public void Pause()
    {
        crater.anim.Pause();
        islands.anim.Pause();
        town.anim.Pause();
        spawnTimer.Stop();

        foreach (Node spawn in GetChildren())
        {
            if (spawn is FlyingObject obj)
            {
                obj.LinearVelocity = new Vector3(0, 0, 0);
                obj.AngularVelocity = new Vector3(0, 0, 0);
                obj.anim.Pause();
            }
        }
    }

    public void IntroDone()
    {
        audioMgr.Play(Audio.GameMusic, AudioChannel.Music);
        game.movementEnabled = true;
        boundaryParticles.Emitting = true;
        player.anim.Play("EaseInHP");
    }

    public void StartSpawn()
    {
        spawnTimer.Start();
    }

    void SpawnObstacle()
    {
        FlyingObject spawn = null;
        switch (rand.RandiRange(0, 4))
        {
            case 0:
                spawn = houseScene.Instantiate<FlyingObject>();
                break;
            case 1:
                spawn = trashcanScene.Instantiate<FlyingObject>();
                break;
            case 2:
                spawn = carScene.Instantiate<FlyingObject>();
                break;
            case 3:
                spawn = toiletScene.Instantiate<FlyingObject>();
                break;
            case 4:
                spawn = sofaScene.Instantiate<FlyingObject>();
                break;
        }
        spawn.Position = GetRandomPosOverFloor(rand.RandiRange(0,1) == 1);
        spawn.RotationDegrees = new Vector3(rand.RandfRange(-180, 180), rand.RandfRange(-180, 180), rand.RandfRange(-180, 180));
        spawn.AngularVelocity = new Vector3(rand.RandfRange(-0.5f, 0.5f), rand.RandfRange(-0.5f, 0.5f), rand.RandfRange(-0.5f, 0.5f));

        AddChild(spawn);
    }

    private Vector3 GetRandomPosOverFloor(bool relationToPlayer)
    {
        float X, Y, Z;
        Y = SPAWN_HEIGHT;
        if (relationToPlayer)
        {
            X = player.Position.X + rand.RandfRange(-SPAWN_RADIUS_AROUND_PLAYER, SPAWN_RADIUS_AROUND_PLAYER);
            Z = player.Position.Z + rand.RandfRange(-SPAWN_RADIUS_AROUND_PLAYER, SPAWN_RADIUS_AROUND_PLAYER);
        }
        else
        {
            X = rand.RandfRange(-SPAWN_RADIUS_PLAY_AREA, SPAWN_RADIUS_PLAY_AREA);
            Z = rand.RandfRange(-SPAWN_RADIUS_PLAY_AREA, SPAWN_RADIUS_PLAY_AREA);
        }
        return new(X, Y, Z);
    }
}
