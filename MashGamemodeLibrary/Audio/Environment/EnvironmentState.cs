using MashGamemodeLibrary.Audio.Containers;

namespace MashGamemodeLibrary.Audio.Players.Background.Music;

public abstract class EnvironmentState<T>
{
    private string[]? _weatherSpawnables;
    private IAudioContainer? _audioContainerCache;
    protected abstract IAudioContainer AudioContainer { get; }
    protected abstract string[] WeatherSpawnables { get; }
    
    public abstract int Priority { get; }
    public abstract bool CanPlay(T context);

    public virtual bool ShouldApplyWeatherEffects(T context)
    {
        return true;
    }
    
    public IAudioContainer GetAudioContainer()
    {
        return _audioContainerCache ??= AudioContainer;
    }
    
    public string[] GetWeatherSpawnables()
    {
        return _weatherSpawnables ??= WeatherSpawnables;
    }
}