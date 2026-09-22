using Il2CppSLZ.Marrow.Combat;
using LabFusion.Player;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IPlayerTakeDamageCallback : IPlayerBehaviour
{
    public void OnDamageTaken(Attack attack, PlayerID? source);
}