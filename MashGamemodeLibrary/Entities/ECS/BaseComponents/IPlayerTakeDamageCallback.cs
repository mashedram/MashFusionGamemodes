using LabFusion.Player;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IPlayerTakeDamageCallback : IPlayerBehaviour
{
    public void OnDamageTaken(PlayerID? source);
}