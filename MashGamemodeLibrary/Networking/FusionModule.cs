using LabFusion.SDK.Gamemodes;
using LabFusion.SDK.Modules;
using MelonLoader;

namespace MashGamemodeLibrary.networking;

internal class FusionModule : Module
{
    public override string Name => "Mash Gamemode Library";
    
    protected override void OnModuleRegistered()
    {
        ModuleMessageManager.RegisterHandler<RemoteEventMessageHandler>();
    }
}