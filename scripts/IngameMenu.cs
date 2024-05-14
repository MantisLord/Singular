using Godot;
using System;

public partial class IngameMenu : Control
{
    private Game game;
	public override void _Ready()
    {
        game = GetNode<Game>("/root/Game");
    }
    private void RestartButtonPressed()
    {
        game.ChangeScene("world");
    }

    private void ResumeButtonPressed()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        Visible = false;
    }

    private static void ExitButtonPressed()
    {
        System.Environment.Exit(1);
    }

    public override void _Process(double delta)
	{
	}
}
