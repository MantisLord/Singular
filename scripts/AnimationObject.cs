using Godot;
using System;

public partial class AnimationObject : Node3D
{
    private Game game;
	public override void _Ready()
    {
        game = GetNode<Game>("/root/Game");
        if (Name == "Intro")
        {
            game.camLookBone = GetNode<BoneAttachment3D>("Intro Rig/Skeleton3D/TV Screen_2");
        }
        base._Ready();
    }

	public void PlayAnimation(string AnimationName)
    {
        var anim = GetNode<AnimationPlayer>("AnimationPlayer");
        anim.Play(AnimationName);
    }

    private void BlackHoleReachedPlayer()
    {
        game.EndGame();
    }

    private void AllowLook()
    {
        game.lookEnabled = true;
    }

    private void IntroDone()
    {
        game.movementEnabled = true;
        World world = GetTree().Root.GetNode<World>("World");
        world.IntroDone();
    }

	public override void _Process(double delta)
	{
	}
}
