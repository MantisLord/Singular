using Godot;
using System;

public partial class PhysicsAnimation : Skeleton3D
{
	public override void _Ready()
    {
        PhysicalBonesStartSimulation();
    }

	public override void _Process(double delta)
	{
	}
}
