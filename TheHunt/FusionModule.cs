using LabFusion.SDK.Gamemodes;
using LabFusion.SDK.Modules;

namespace TheHunt;

public class FusionModule : Module
{
    public override string Name => "TheHunt";

    protected override void OnModuleRegistered()
    {
        GamemodeRegistration.RegisterGamemode<Gamemode.TheHunt>();
    }
}