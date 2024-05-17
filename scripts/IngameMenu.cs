using Godot;

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

    private void RestartButtonPressed()
    {
        world.Restart();
        Input.MouseMode = Input.MouseModeEnum.Captured;
        menu.Visible = false;
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

    private static void ExitButtonPressed()
    {
        System.Environment.Exit(1);
    }

    public override void _Process(double delta)
    {
    }
}
