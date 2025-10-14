using LabFusion.SDK.Gamemodes;
using LabFusion.SDK.Modules;

namespace BoneStrike;

public class FusionModule : Module
{
    public override string Name => "BoneStrike";

    protected override void OnModuleRegistered()
    {
        GamemodeRegistration.RegisterGamemode<BoneStrike>();
    }
}