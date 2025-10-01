using MashGamemodeLibrary.Audio.Containers;

namespace MashGamemodeLibrary.Audio.Players.Background.Music;

public abstract class MusicState<T>
{
    private IAudioContainer? _audioContainerCache;
    protected abstract IAudioContainer AudioContainer { get; }
    
    public abstract int Priority { get; }
    public abstract bool CanPlay(T context);
    
    public IAudioContainer GetAudioContainer()
    {
        if (_audioContainerCache == null)
            _audioContainerCache = AudioContainer;
        return _audioContainerCache;
    }
}