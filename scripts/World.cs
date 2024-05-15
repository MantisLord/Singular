using Godot;
using static AudioManager;

public partial class World : Node3D
{
    private Timer spawnTimer;
    private Timer houseTimer;
    private Timer holeExpandTimer;
    private PackedScene blockScene = GD.Load<PackedScene>("res://scenes/block.tscn");
    private PackedScene houseScene = GD.Load<PackedScene>("res://scenes/house_physics.tscn");
    private int numSpawns = 0;
    private MeshInstance3D spawner;
    private Aabb spawnerBox;
    private Player player;
    private AnimationObject crater;
    private AnimationObject intro;
    private RandomNumberGenerator rand;
    private AudioManager audioMgr;
    private Game game;
    private GpuParticles3D boundaryParticles;

    public override void _Ready()
    {
        audioMgr = GetNode<AudioManager>("/root/AudioManager");
        game = GetNode<Game>("/root/Game");

        spawnTimer = GetNode<Timer>("SpawnTimer");
        spawnTimer.Timeout += () => SpawnObstacle();
        spawner = GetNode<MeshInstance3D>("Spawner");
        spawnerBox = spawner.GetAabb();
        player = GetNode<Player>("Player");
        crater = GetNode<AnimationObject>("Crater");
        intro = GetNode<AnimationObject>("Intro");
        rand = new RandomNumberGenerator();
        boundaryParticles = GetNode<GpuParticles3D>("BoundaryParticles");

        audioMgr.Play(Audio.Opening, AudioChannel.Ambient);
        intro.PlayAnimation("ArmatureAction");
        crater.PlayAnimation("Crater RigAction");
    }

	public override void _Process(double delta)
	{

    }

    public void Restart()
    {
        game.lookEnabled = true;
        player.healthProgressBar.Value = 100;
        audioMgr.Stop();
        intro.PlayAnimation("ArmatureAction", 60);
        crater.PlayAnimation("Crater RigAction", 60);
        IntroDone();
    }

    public void IntroDone()
    {
        audioMgr.Play(Audio.GameMusic, AudioChannel.Music);
        game.movementEnabled = true;
        boundaryParticles.Emitting = true;
        spawnTimer.Start();
    }

    void SpawnObstacle()
    {
        FlyingObject spawn = null;
        switch (rand.RandiRange(0,1))
        {
            case 0:
                spawn = blockScene.Instantiate<FlyingObject>();
                break;
            case 1:
                spawn = houseScene.Instantiate<FlyingObject>();
                break;
        }
        spawn.Position = GetRandomPosOverFloor();
        AddChild(spawn);
        numSpawns++;
        player.SetNumSpawns(numSpawns);
    }

    private Vector3 GetRandomPosOverFloor()
    {
        float X = spawner.Position.X + rand.RandfRange(-spawnerBox.Size.X / 2, spawnerBox.Size.X / 2);
        float Y = spawner.Position.Y;
        float Z = spawner.Position.Z + rand.RandfRange(-spawnerBox.Size.Z / 2, spawnerBox.Size.Z / 2);
        return new(X, Y, Z);
    }
}
