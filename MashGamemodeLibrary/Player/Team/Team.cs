using LabFusion.Player;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Player.Team;

public abstract class Team
{
    public abstract string Name { get; }
    public virtual uint Capacity => UInt32.MaxValue;
    public virtual uint Weight => 1;

    public virtual void OnAssigned(PlayerID player)
    {
    }

    public virtual void OnRemoved(PlayerID player)
    {
    }
}