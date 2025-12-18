using BoneStrike.Teams;
using MashGamemodeLibrary.Entities.Tagging.Base;
using MashGamemodeLibrary.Phase;

namespace BoneStrike.Tags;

public class BombMarker : EntityTag, ITagRemoved
{
    public void OnRemoval(ushort entityID)
    {
        WinManager.Win<CounterTerroristTeam>();
    }
}