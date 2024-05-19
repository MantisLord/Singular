using Godot;

public partial class AudioManager : Node
{
    AudioStreamPlayer sfxStreamPlayer1 = new();
    AudioStreamPlayer sfxStreamPlayer2 = new();
    AudioStreamPlayer sfxStreamPlayer3 = new();
    AudioStreamPlayer ambienceStreamPlayer = new();
    AudioStreamPlayer musicStreamPlayer = new();
    AudioStream asOpening, asGame, asClosing, asFailure, asRat;
    AudioStream asButtonHover, asButtonPlay, asButtonOptions, asButtonRestart, asButtonQuit;
    RandomNumberGenerator rand = new();

    private const int VOLUME_ADJUSTMENT = -20;

    public enum AudioChannel
    {
        SFX1,
        SFX2,
        SFX3,
        Ambient,
        Music
    }

    public enum Audio
    {
        Opening,
        GameMusic,
        Closing,
        Failure,
        Rat,
        ButtonHover,
        ButtonPlay,
        ButtonOptions,
        ButtonRestart,
        ButtonQuit,
    }

    public override void _Ready()
    {
        base._Ready();
        asOpening = GD.Load<AudioStream>("res://audio/music/opening.ogg");
        asGame = GD.Load<AudioStream>("res://audio/music/game.ogg");
        asClosing = GD.Load<AudioStream>("res://audio/music/closing.ogg");
        asFailure = GD.Load<AudioStream>("res://audio/music/failure.ogg");
        asRat = GD.Load<AudioStream>("res://audio/music/rat.wav");

        asButtonHover = GD.Load<AudioStream>("res://audio/interface/interface - hover.wav");
        asButtonOptions = GD.Load<AudioStream>("res://audio/interface/interface - OPTIONS.wav");
        asButtonPlay = GD.Load<AudioStream>("res://audio/interface/interface - PLAY.wav");
        asButtonQuit = GD.Load<AudioStream>("res://audio/interface/interface - quit.wav");
        asButtonRestart = GD.Load<AudioStream>("res://audio/interface/interface - RESTART.wav");
        AddChild(sfxStreamPlayer1);
        AddChild(sfxStreamPlayer2);
        AddChild(sfxStreamPlayer3);
        AddChild(ambienceStreamPlayer);
        AddChild(musicStreamPlayer);
        rand.Randomize();
    }

    public void Play(AudioStreamPlayer3D player, AudioChannel channel = AudioChannel.SFX1)
    {
        // adjust volume based on channel
        player.VolumeDb = VOLUME_ADJUSTMENT;
        player.Play();
    }

    public void Play(Audio sound, AudioChannel channel = AudioChannel.SFX1, bool await = false, int fromPos = 0)
    {
        AudioStream stream = null;
        AudioStreamPlayer currentPlayer = null;

        switch (channel)
        {
            case AudioChannel.SFX1:
                currentPlayer = sfxStreamPlayer1;
                break;
            case AudioChannel.SFX2:
                currentPlayer = sfxStreamPlayer2;
                break;
            case AudioChannel.SFX3:
                currentPlayer = sfxStreamPlayer3;
                break;
            case AudioChannel.Ambient:
                currentPlayer = ambienceStreamPlayer;
                break;
            case AudioChannel.Music:
                currentPlayer = musicStreamPlayer;
                break;
        }
        switch (sound)
        {
            case Audio.Opening:
                stream = asOpening;
                break;
            case Audio.GameMusic:
                stream = asGame;
                break;
            case Audio.Closing:
                stream = asClosing;
                break;
            case Audio.Failure:
                stream = asFailure;
                break;
            case Audio.Rat:
                stream = asRat;
                break;
            case Audio.ButtonHover:
                stream = asButtonHover;
                break;
            case Audio.ButtonPlay:
                stream = asButtonPlay;
                break;
            case Audio.ButtonQuit:
                stream = asButtonQuit;
                break;
            case Audio.ButtonRestart:
                stream = asButtonRestart;
                break;
            case Audio.ButtonOptions:
                stream = asButtonOptions;
                break;
        }
        currentPlayer.VolumeDb = VOLUME_ADJUSTMENT;
        currentPlayer.Stream = stream;
        currentPlayer.Play(fromPos);
        if (await)
            System.Threading.Thread.Sleep((int)(stream.GetLength() * 1000));
    }

    public void Stop()
    {
        sfxStreamPlayer1.Stop();
        sfxStreamPlayer2.Stop();
        sfxStreamPlayer3.Stop();
        ambienceStreamPlayer.Stop();
        musicStreamPlayer.Stop();
    }

    public void StopBG()
    {
        ambienceStreamPlayer.Stop();
        musicStreamPlayer.Stop();
    }

    public bool IsMusicPlaying()
    {
        return musicStreamPlayer.Playing;
    }
}
