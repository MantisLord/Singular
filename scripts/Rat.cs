using Godot;
using System;

public partial class Rat : Node3D
{
	public override void _Ready()
    {
        var anim = GetNode<AnimationPlayer>("AnimationPlayer");
        anim.Play("Walk");
        base._Ready();
    }

	public override void _Process(double delta)
	{
	}
}
