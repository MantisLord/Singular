using Godot;

public partial class Game : Node
{
    public bool movementEnabled = false;
    public bool lookEnabled = false;
    public BoneAttachment3D camLookBone;

    public void ChangeScene(string sceneName)
    {
        GetTree().ChangeSceneToFile($"scenes/{sceneName}.tscn");
    }
}
