using Godot;

public partial class AudioManager : Node
{
    AudioStreamPlayer sfxStreamPlayer1 = new();
    AudioStreamPlayer sfxStreamPlayer2 = new();
    AudioStreamPlayer sfxStreamPlayer3 = new();
    AudioStreamPlayer ambienceStreamPlayer = new();
    AudioStreamPlayer musicStreamPlayer = new();
    AudioStream asOpening, asGame, asClosing;
    RandomNumberGenerator rand = new();

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
    }

    public override void _Ready()
    {
        base._Ready();
        asOpening = GD.Load<AudioStream>("res://audio/music/opening.ogg");
        asGame = GD.Load<AudioStream>("res://audio/music/game.ogg");
        asClosing = GD.Load<AudioStream>("res://audio/music/closing.ogg");
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
        //player.VolumeDb = 
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
        }
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

    public bool IsMusicPlaying()
    {
        return musicStreamPlayer.Playing;
    }
}
