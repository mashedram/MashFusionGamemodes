using BoneStrike.Phase;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.Phase;
using Team = MashGamemodeLibrary.Player.Team.Team;

namespace BoneStrike.Teams;

public class CounterTerroristTeam : Team
{
    public override string Name => "Counter Terrorists";
    public override uint Capacity => UInt32.MaxValue;
    public override uint Weight => 0;

    public override void OnPhaseChanged(GamePhase phase)
    {
        LocalVision.Blind = phase is PlantPhase;
    }

    public override void OnRemoved()
    {
        LocalVision.Blind = false;
    }
}