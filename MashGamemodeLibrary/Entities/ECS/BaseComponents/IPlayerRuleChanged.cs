using LabFusion.Entities;
using MashGamemodeLibrary.Player.Spectating.data.Rules;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IPlayerRuleChanged : IPlayerBehaviour
{
    void OnPlayerRuleChanged(NetworkPlayer networkPlayer, IPlayerRule newRule);
}