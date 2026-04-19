using LabFusion.Entities;
using LabFusion.Network;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Util;
using MelonLoader;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Team;

public abstract class LogicTeam
{
    public abstract string Name { get; }
    public virtual Texture? Icon => null;

    private NetworkPlayer? _owner;
    public NetworkPlayer Owner => _owner ?? throw new InvalidOperationException($"No player is currently assigned to team: {Name}!");

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

        if (LobbyInfoManager.LobbyInfo.NameTags)
        {
            player.Icon.Texture = Icon;
            player.Icon.Visible = Icon != null;
        }

        _owner = player;
        Executor.RunChecked(OnAssigned);
    }

    internal void Remove()
    {
        if (_owner == null)
        {
            InternalLogger.Debug($"Attempted to remove player from team: {Name}, but no player was assigned!");
            return;
        }

        InternalLogger.Debug($"Player: {Owner.Username} left team: {Name}");
        Owner.Icon.Texture = null;
        Owner.Icon.Visible = false;

        Executor.RunChecked(OnRemoved);
    }
}