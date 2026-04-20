using MashGamemodeLibrary.Audio.Players.Extensions;

namespace MashGamemodeLibrary.Environment.Effector;

public abstract class CustomAudioEffector<TContext, TPlayer> : EnvironmentEffector<TContext> where TPlayer : IAudioPlayer
{
    private readonly TPlayer _audioPlayer;

    public CustomAudioEffector(TPlayer audioPlayer)
    {
        _audioPlayer = audioPlayer;
    }

    protected abstract void Play(TPlayer audioPlayer, TContext context);
    

    public override void Apply(TContext context)
    {
        Play(_audioPlayer, context);
    }

    public override void Update(TContext context, float delta)
    {
        _audioPlayer.Update(delta);
    }

    public override void Remove(TContext context)
    {
        _audioPlayer.Stop();
    }
}