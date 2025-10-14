using LabFusion.SDK.Gamemodes;
using LabFusion.SDK.Modules;

namespace Clockhunt;

public class FusionModule : Module
{
    public override string Name => "Clockhunt";

    protected override void OnModuleRegistered()
    {
        GamemodeRegistration.RegisterGamemode<Clockhunt>();
    }
}