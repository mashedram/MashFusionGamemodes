using LabFusion.Entities;
using LabFusion.Player;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Player.Team;

public abstract class Team
{
    public abstract string Name { get; }
    public virtual uint Capacity => UInt32.MaxValue;
    public virtual uint Weight => 1;

    public NetworkPlayer Owner { get; internal set; } = null!;

    public virtual void OnAssigned()
    {
    }

    public virtual void OnPhaseChanged(GamePhase phase)
    {
        
    }

    public virtual void OnRemoved()
    {
    }

    internal void Assign(NetworkPlayer player)
    {
        Owner = player;
        OnAssigned();
    }
    
    internal void Remove()
    {
        OnRemoved();
    }
}