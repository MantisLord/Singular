using Godot;

public partial class IngameMenu : Control
{
    private Game game;
    private World world;
    public override void _Ready()
    {
        game = GetNode<Game>("/root/Game");
        world = GetTree().Root.GetNode<World>("World");
    }

    private void RestartButtonPressed()
    {
        world.Restart();
        Input.MouseMode = Input.MouseModeEnum.Captured;
        Visible = false;
    }

    private void MainMenuButtonPressed()
    {
        game.EndGame();
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
