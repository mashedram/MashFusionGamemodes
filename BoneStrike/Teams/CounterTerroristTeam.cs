using MashGamemodeLibrary.Player.Team;

namespace BoneStrike.Teams;

public class CounterTerroristTeam : Team
{
    public override string Name => "Counter Terrorists";
    public override uint Capacity => UInt32.MaxValue;
    public override uint Weight => 0;
}