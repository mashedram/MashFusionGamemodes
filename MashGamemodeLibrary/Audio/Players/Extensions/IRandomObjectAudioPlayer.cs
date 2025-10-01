using LabFusion.Entities;

namespace MashGamemodeLibrary.Audio.Players.Extensions;

public interface IRandomObjectAudioPlayer : IRandomAudioPlayer
{
    public void PlayRandomAt(NetworkEntity entity);
}