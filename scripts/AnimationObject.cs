using Godot;

public partial class AnimationObject : Node3D
{
    private Game game;
    private World world;
    public AnimationPlayer anim;
    public override void _Ready()
    {
        game = GetNode<Game>("/root/Game");
        world = GetTree().Root.GetNode<World>("World");
        if (Name == "Intro")
        {
            game.camLookBone = GetNode<BoneAttachment3D>("Intro Rig/Skeleton3D/TV_2");
        }
        else if (Name == "Outro")
        {
            game.camLookBoneOutro = GetNode<BoneAttachment3D>("House Rig/Skeleton3D/TV -col");
        }
        anim = GetNode<AnimationPlayer>("AnimationPlayer");
        base._Ready();
    }

    public void PlayAnimation(string AnimationName, double seekTime = 0)
    {
        anim.Play(AnimationName);
        if (seekTime > 0)
            anim.Seek(seekTime);
    }

    // Intro Anim Events
    private void StartGameAnims()
    {
        world.StartGameAnims();
    }
    private void AllowLook()
    {
        game.lookEnabled = true;
    }
    private void StartGameMusic()
    {
        world.StartGameMusic();
    }
    private void GainControl()
    {
        world.GainControl();
    }
    private void StartSpawn()
    {
        world.StartSpawn();
    }

    // Crater Anim Events
    private void StopSpawn()
    {
        world.StopSpawn();
    }
    private void BlackHoleReachedPlayer()
    {
        world.Won();
    }

    public override void _Process(double delta)
    {
    }
}
