using LabFusion.Player;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Player.Team;

public abstract class Team
{
    public abstract string Name { get; }
    public abstract uint Capacity { get; }
    public abstract uint Weight { get; }

    public virtual void OnAssigned(PlayerID player)
    {
    }

    public virtual void OnRemoved(PlayerID player)
    {
    }
}