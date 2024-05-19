using Godot;
using static AudioManager;

public partial class IngameMenu : Control
{
    private Game game;
    private AudioManager audioMgr;
    private World world;
    private PanelContainer menu;
    public override void _Ready()
    {
        game = GetNode<Game>("/root/Game");
        audioMgr = GetNode<AudioManager>("/root/AudioManager");
        world = GetTree().Root.GetNode<World>("World");
        menu = GetNode<PanelContainer>("MenuContainer");
    }

    private void ButtonHover()
    {
        audioMgr.Play(Audio.ButtonHover, AudioChannel.SFX2);
    }

    private void RestartButtonPressed()
    {
        world.Restart();
        Input.MouseMode = Input.MouseModeEnum.Captured;
        menu.Visible = false;
        audioMgr.Play(Audio.ButtonRestart, AudioChannel.SFX3);
    }

    private void MainMenuButtonPressed()
    {
        audioMgr.Stop();
        game.ChangeScene("main_menu");
    }

    private void ResumeButtonPressed()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        menu.Visible = false;
    }

    private void ExitButtonPressed()
    {
        if (!game.won && game.gameOver)
        {
            audioMgr.Play(Audio.ButtonQuit, AudioChannel.SFX3, true);
            System.Environment.Exit(1);
        }
        else
            world.EasterEgg();
    }

    public override void _Process(double delta)
    {
    }
}
