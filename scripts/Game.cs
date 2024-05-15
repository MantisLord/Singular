using Godot;

public partial class Game : Node
{
    public bool movementEnabled = false;
    public bool lookEnabled = false;
    private AudioManager audioMgr;
    public BoneAttachment3D camLookBone;
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
