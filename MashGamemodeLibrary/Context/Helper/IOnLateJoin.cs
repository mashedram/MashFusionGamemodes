using LabFusion.Player;

namespace MashGamemodeLibrary.Context.Helper;

public interface IOnLateJoin
{
    void OnLateJoin(PlayerID playerID);
}