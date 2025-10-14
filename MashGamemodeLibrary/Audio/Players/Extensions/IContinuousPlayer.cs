namespace MashGamemodeLibrary.Audio.Players.Extensions;

public interface IContinuousPlayer : IAudioPlayer
{
    bool IsActive { get; }
    void StartPlaying();
    void StopPlaying();
    void Update(float delta);
}