namespace MashGamemodeLibrary.Audio.Players.Extensions;

public interface IRandomAudioPlayer : IAudioPlayer
{
    string GetRandomAudioName();
}

public interface IRandomAudioPlayer<in T> : IRandomAudioPlayer
{
    void PlayRandom(T parameter);
}