using MashGamemodeLibrary.Phase.Rounds;

namespace Clockhunt;

public class ClockhuntRound : RoundContext
{

    public override int RoundCount => 1;
    public override float RoundCooldown => 15f;
}