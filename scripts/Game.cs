using Godot;
using Godot.Collections;

public partial class Game : Node
{
    public bool won = false;
    public bool gameOver = false;
    public bool movementEnabled = false;
    public bool lookEnabled = false;
    public bool outroPlaying = false;
    public BoneAttachment3D camLookBone;
    public BoneAttachment3D camLookBoneOutro;

    public enum FlyingObjectName
    {
        House,
        Car,
        Sofa,
        Lamp,
        Toilet,
        TrashCan,
    }

    public void ChangeScene(string sceneName)
    {
        GetTree().ChangeSceneToFile($"scenes/{sceneName}.tscn");
    }
}
