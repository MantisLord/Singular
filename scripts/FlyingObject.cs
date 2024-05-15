using Godot;

public partial class FlyingObject : RigidBody3D
{
    [Export] public int DamageDealt = 10;
    AnimationPlayer anim = new();
    public override void _Ready()
    {
        anim = GetNode<AnimationPlayer>("AnimationPlayer");
        anim.Play("scaleUp");
    }

    private void StartShrink()
    {
        anim.Play("scaleDown");
    }

    private void Despawn()
    {
        QueueFree();
    }

    private void Collided(Node body)
    {
        GD.Print($"Hit {body.Name}");
        if (body is Player player)
            player.TakeHit(DamageDealt);
    }

    public override void _Process(double delta)
    {
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }
}
