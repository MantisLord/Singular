using Godot;
using System;

public partial class HouseAnimated : Node3D
{
	public override void _Ready()
	{
        var anim = GetNode<AnimationPlayer>("AnimationPlayer");
        anim.Play("ArmatureAction");
        base._Ready();
    }

	public override void _Process(double delta)
	{
	}
}
