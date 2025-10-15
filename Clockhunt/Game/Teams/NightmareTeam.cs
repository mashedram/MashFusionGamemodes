using Clockhunt.Config;
using Clockhunt.Nightmare;
using LabFusion.Player;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player.Team;

namespace Clockhunt.Game.Teams;

public class NightmareTeam : Team
{
    public override string Name => "Nightmare";
    public override uint Capacity => 1;

    public override void OnAssigned(PlayerID player)
    {
        Executor.RunIfHost(() =>
        {
            NightmareManager.SetRandomNightmare(player);
        });
    }

    public override void OnRemoved(PlayerID player)
    {
        Executor.RunIfHost(() =>
        {
            NightmareManager.RemoveNightmare(player);
        });
    }
}