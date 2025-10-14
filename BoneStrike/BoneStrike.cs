using MashGamemodeLibrary.Context;

namespace BoneStrike;

public class BoneStrike : GamemodeWithContext<BoneStrikeContext>
{
    public override string Title => "Bone Strike";
    public override string Author => "Mash";

    public override void OnGamemodeRegistered()
    {
        BoneStrikeContext.TeamManager.Register(this);
        BoneStrikeContext.TeamManager.AddTeam(BoneStrikeContext.Terrorists);
        BoneStrikeContext.TeamManager.AddTeam(BoneStrikeContext.CounterTerrorists);
    }
}