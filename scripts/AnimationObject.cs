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
            game.camLookBone = GetNode<BoneAttachment3D>("Intro Rig/Skeleton3D/TV Screen_2");
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
    private void BlackHoleReachedPlayer()
    {
        game.EndGame();
    }

    public override void _Process(double delta)
    {
    }
}
