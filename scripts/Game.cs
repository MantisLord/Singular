using Godot;

public partial class Game : Node
{
    public bool movementEnabled = false;
    public bool lookEnabled = false;
    private AudioManager audioMgr;
    public BoneAttachment3D camLookBone;
    public bool gameOver = false;
    public override void _Ready()
    {
        audioMgr = GetNode<AudioManager>("/root/AudioManager");
    }

    public void EndGame()
    {
        gameOver = true;
        movementEnabled = false;
        lookEnabled = false;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        audioMgr.Stop();
    }

    public void ChangeScene(string sceneName)
    {
        GetTree().ChangeSceneToFile($"scenes/{sceneName}.tscn");
    }

    public override void _Process(double delta)
    {
    }
}
