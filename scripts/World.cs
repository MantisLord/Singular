using Godot;
using System;
using System.Drawing;

public partial class World : Node3D
{
    private Timer spawnTimer;
    private PackedScene blockScene = GD.Load<PackedScene>("res://scenes/block.tscn");
    private PackedScene houseScene = GD.Load<PackedScene>("res://scenes/house_physics.tscn");
    private int numSpawns = 0;
    private MeshInstance3D spawner;
    private Aabb spawnerBox;
    private Player player;
    RandomNumberGenerator rand;

    public override void _Ready()
	{
        spawnTimer = GetNode<Timer>("SpawnTimer");
        spawnTimer.Timeout += () => SpawnObstacle();
        spawner = GetNode<MeshInstance3D>("Spawner");
        spawnerBox = spawner.GetAabb();
        player = GetNode<Player>("Player");
        rand = new RandomNumberGenerator();
    }

	public override void _Process(double delta)
	{
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
