using LabFusion.Player;
using LabFusion.Senders;
using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IPlayerActionCallback : IBehaviour
{
    void OnAction(PlayerActionType action, PlayerID otherPlayer);
}