using MashGamemodeLibrary.Audio.Players.Extensions;

namespace MashGamemodeLibrary.Environment.Effector;

public abstract class AudioEffector<TContext> : EnvironmentEffector<TContext>
{
    private readonly IContinuousPlayer _audioContainer;

    public AudioEffector(IContinuousPlayer audioContainer)
    {
        _audioContainer = audioContainer;
    }


    public override void Apply(TContext context)
    {
        _audioContainer.StartPlaying();
    }

    public override void Update(TContext context, float delta)
    {
        _audioContainer.Update(delta);
    }

    public override void Remove(TContext context)
    {
        _audioContainer.StopPlaying();
    }
}