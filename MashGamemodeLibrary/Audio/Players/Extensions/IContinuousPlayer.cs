namespace MashGamemodeLibrary.Audio.Players.Extensions;

public interface IContinuousPlayer : IAudioPlayer
{
    bool IsActive { get; }
    void Start();
    void Stop();
    void Update(float delta);
}