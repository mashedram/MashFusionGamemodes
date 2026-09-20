using LabFusion.SDK.Gamemodes;
using LabFusion.SDK.Modules;

namespace Chaos;

public class FusionModule : Module
{
    public override string Name => "Chaos";

    protected override void OnModuleRegistered()
    {
        GamemodeRegistration.RegisterGamemode<Chaos>();
    }
}