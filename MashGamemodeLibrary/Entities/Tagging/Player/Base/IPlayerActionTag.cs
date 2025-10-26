using LabFusion.Player;
using LabFusion.Senders;

namespace MashGamemodeLibrary.Entities.Tagging.Player.Base;

public interface IPlayerActionTag
{
    void OnAction(PlayerActionType action, PlayerID otherPlayer);
}