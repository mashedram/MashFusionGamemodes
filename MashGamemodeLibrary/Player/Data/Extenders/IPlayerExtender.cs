using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Data.Events;
using MashGamemodeLibrary.Player.Data.Events.Data;
using MashGamemodeLibrary.Player.Spectating.data.Rules;

namespace MashGamemodeLibrary.Player.Data.Extenders;

public interface IPlayerExtender
{
    void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager);
    void OnRuleChanged(IPlayerRule rule);
    void OnEvent(IPlayerEvent playerEvent);
}