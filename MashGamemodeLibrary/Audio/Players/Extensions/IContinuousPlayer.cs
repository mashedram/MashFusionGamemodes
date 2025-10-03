namespace MashGamemodeLibrary.Audio.Players.Extensions;

public interface IContinuousPlayer
{
    void StartPlaying();
    void StopPlaying();
    bool IsActive { get; }
    void Update(float delta);
}