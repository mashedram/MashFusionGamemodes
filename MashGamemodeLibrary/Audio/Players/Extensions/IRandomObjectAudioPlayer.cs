using LabFusion.Entities;

namespace MashGamemodeLibrary.Audio.Players.Extensions;

public interface IRandomObjectAudioPlayer : IRandomAudioPlayer
{
    void PlayRandomAt(NetworkEntity entity);
    void StopAll();
}