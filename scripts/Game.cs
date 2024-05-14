using Godot;
using System;

public partial class Game : Node
{
    public bool movementEnabled = false;
	private AudioManager audioMgr;
    public override void _Ready()
    {
        audioMgr = GetNode<AudioManager>("/root/AudioManager");
    }

	public void EndGame()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
		audioMgr.Stop();
        ChangeScene("main_menu");
    }

	public void ChangeScene(string sceneName)
	{
		GetTree().ChangeSceneToFile($"scenes/{sceneName}.tscn");
	}

	public override void _Process(double delta)
	{
	}
}
