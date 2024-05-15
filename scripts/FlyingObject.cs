using Godot;
using System;

public partial class FlyingObject : RigidBody3D
{
    [Export] public int DamageDealt = 10;
    private const float SCALE_TIME = 5.0f;
    public override void _Ready()
    {
        //Scale = Vector3.Zero;
        //Tween scaleUpTween = GetTree().CreateTween();
        //scaleUpTween.TweenProperty(this, "scale", new Vector3(1.0f, 1.0f, 1.0f), SCALE_TIME);
        //scaleUpTween.Play();
        //Tween scaleDownTween = GetTree().CreateTween();
        //scaleDownTween.TweenProperty(this, "scale", Vector3.Zero, SCALE_TIME).SetDelay(SCALE_TIME);
        //scaleDownTween.Play();
    }

    private void Collided(Node body)
    {
        GD.Print($"Hit {body.Name}");
        if (body is Player player)
            player.TakeHit(DamageDealt);
    }

	public override void _Process(double delta)
	{
        //Vector3 scale = new(Scale.X , Scale.Y, Scale.Z);
        //scale.X = Mathf.Lerp(Scale.X, 1.0f, (float)delta * SCALE_TIME);
        //scale.Y = Mathf.Lerp(Scale.Y, 1.0f, (float)delta * SCALE_TIME);
        //scale.Z = Mathf.Lerp(Scale.Z, 1.0f, (float)delta * SCALE_TIME);
        //Scale = scale;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }
}
