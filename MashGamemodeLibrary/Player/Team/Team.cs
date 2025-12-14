using LabFusion.Entities;
using LabFusion.Player;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Player.Team;

public abstract class Team
{
    public abstract string Name { get; }
    public NetworkPlayer Owner { get; private set; } = null!;

    protected virtual void OnAssigned()
    {
    }

    public virtual void OnPhaseChanged(GamePhase phase)
    {
        
    }

    protected virtual void OnRemoved()
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