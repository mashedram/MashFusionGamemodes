namespace MashGamemodeLibrary.Audio.Players.Extensions;

public interface IContinuousPlayer : IAudioPlayer
{
    bool IsActive { get; }
    void Start();
}