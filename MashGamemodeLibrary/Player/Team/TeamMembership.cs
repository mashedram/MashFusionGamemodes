using LabFusion.Entities;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Util;
using MelonLoader;

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
        InternalLogger.Debug($"Player: {player.Username} joined team: {Name}");
        
        Owner = player;
        Executor.RunChecked(OnAssigned);
    }

    internal void Remove()
    {
        InternalLogger.Debug($"Player: {Owner.Username} left team: {Name}");
        
        Executor.RunChecked(OnRemoved);
    }
}