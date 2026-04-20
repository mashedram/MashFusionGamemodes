using MashGamemodeLibrary.Player.Data;
using MashGamemodeLibrary.Player.Data.Rules.Rules;

namespace MashGamemodeLibrary.Player.Helpers;

public static class CrippleHelper
{
    public static bool IsCrippled {
        get
        {
            var data = PlayerDataManager.GetLocalPlayerData();
            if (data == null)
                return false;
            
            return data.CheckRule<PlayerCrippledRule>(p => p.IsEnabled) || 
                   data.CheckRule<PlayerSpectatingRule>(p => p.IsSpectating);
        }
    }
}