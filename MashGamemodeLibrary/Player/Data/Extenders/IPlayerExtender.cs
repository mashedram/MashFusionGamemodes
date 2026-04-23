using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Data.Events.Data;

namespace MashGamemodeLibrary.Player.Data.Extenders;

public interface IPlayerExtender
{
    IEnumerable<Type> RuleTypes => Array.Empty<Type>();
    IEnumerable<Type> EventTypes => Array.Empty<Type>();
    
    void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager);
    /// <summary>
    /// Don't forget to mark the rules you wish to listen to
    /// </summary>
    /// <param name="data"></param>
    void OnRuleChanged(PlayerData data);
    /// <summary>
    /// Don't forget to mark the events you wish to listen to
    /// </summary>
    /// <param name="playerEvent"></param>
    void OnEvent(IPlayerEvent playerEvent);
}