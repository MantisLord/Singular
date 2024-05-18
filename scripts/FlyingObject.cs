using Godot;

public partial class FlyingObject : RigidBody3D
{
    [Export] public string ObjectName = "FlyingObject";
    [Export] public int DamageDealt = 10;
    [Export] public AudioStreamPlayer3D CollisionAudio = new();
    [Export] public float Speed = 1.0f; // 10 m/s - 3 sec animation - default (1 scale)
    public AnimationPlayer anim = new();
    CollisionShape3D collisionShape;
    Player player;
    public override void _Ready()
    {
        anim = GetNode<AnimationPlayer>("AnimationPlayer");
        anim.SpeedScale = Speed;
        LinearVelocity = new Vector3(0, LinearVelocity.Y * Speed, 0);
        anim.Play($"flyingObjectAnim/scaleUp{(ObjectName == "House" ? "5x" : "")}");
        collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
        player = GetParent().GetNode<Player>("Player");
    }

    private void StartShrink()
    {
        anim.Play($"flyingObjectAnim/scaleDown{(ObjectName == "House" ? "5x" : "")}");
    }

    private void Despawn()
    {
        QueueFree();
    }

    private void Collided(Node body)
    {
        GD.Print($"{ObjectName} hit {body.Name}");
        if (body is Player player)
            player.TakeHit(DamageDealt, CollisionAudio, ObjectName);
    }

    public override void _Process(double delta)
    {
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }
}
