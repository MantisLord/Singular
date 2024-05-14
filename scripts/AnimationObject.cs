using Godot;
using System;

public partial class AnimationObject : Node3D
{
    private Game game;
	public override void _Ready()
    {
        game = GetNode<Game>("/root/Game");
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

	public override void _Process(double delta)
	{
	}
}
