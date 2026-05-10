using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Behaviour.Cache;

namespace MashGamemodeLibrary.Entities.Interaction.Attacking;

public interface IAttackReceiver : IBehaviour
{
    void ReceiveAttack(float damage, NetworkPlayer attacker);
}