using Godot;
using System;

public partial class FlyingObject : RigidBody3D
{
    private const float SCALE_TIME = 1.0f;
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
	public override void _Process(double delta)
	{
	}

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }
}
