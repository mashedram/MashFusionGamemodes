namespace MashGamemodeLibrary.Audio.Players.Extensions;

public interface IContinuousPlayer : IAudioPlayer
{
    void StartPlaying();
    void StopPlaying();
    bool IsActive { get; }
    void Update(float delta);
}