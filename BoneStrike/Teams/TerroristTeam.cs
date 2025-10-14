using MashGamemodeLibrary.Player.Team;

namespace BoneStrike.Teams;

public class TerroristTeam : Team
{
    public override string Name => "Terrorists";
    public override uint Capacity => UInt32.MaxValue;
    public override uint Weight => 1;
}