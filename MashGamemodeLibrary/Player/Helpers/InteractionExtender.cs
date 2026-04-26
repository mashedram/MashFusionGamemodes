using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.Scene;
using MashGamemodeLibrary.Player.Data;
using MashGamemodeLibrary.Player.Data.Rules.Rules;

namespace MashGamemodeLibrary.Player.Helpers;

public static class InteractionExtender
{
    // TODO : Use a player interaction system instead to seperate that from spectating for cool abilities in gamemodes
    public static bool HasInteractions(this NetworkPlayer player)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
            return true;
        
        return PlayerDataManager.GetPlayerData(player)?.CheckRule<PlayerSpectatingRule>(r => r.IsSpectating) ?? true;
    }
    
    public static bool HasLocalInteractions()
    {
        if (!NetworkSceneManager.IsLevelNetworked)
            return true;
        
        return PlayerDataManager.GetLocalPlayerData()?.CheckRule<PlayerSpectatingRule>(r => r.IsSpectating) ?? true;
    }
}