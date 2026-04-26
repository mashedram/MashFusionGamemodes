using LabFusion.Entities;
using MashGamemodeLibrary.Player.Data.Rules;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IPlayerRuleChangedCallback : IPlayerBehaviour
{
    void OnPlayerRuleChanged(NetworkPlayer networkPlayer, IPlayerRule newRule);
}