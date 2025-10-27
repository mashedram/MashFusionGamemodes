using MashGamemodeLibrary.Phase.Rounds;

namespace BoneStrike;

public class BonestrikeRound : RoundContext
{
    public override int RoundCount => 5;
    public override float RoundCooldown => 30f;
}