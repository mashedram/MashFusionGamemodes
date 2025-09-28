using MashGamemodeLibrary.Audio.Containers;

namespace MashGamemodeLibrary.Audio.Players.Background.Music;

public abstract class MusicState<T>
{
    public abstract IAudioContainer AudioContainer { get; }
    public abstract int Priority { get; }
    public abstract bool CanPlay(T context);
}