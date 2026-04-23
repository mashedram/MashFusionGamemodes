using LabFusion.SDK.Modules;
using MashGamemodeLibrary.Networking.Remote;

namespace MashGamemodeLibrary.networking;

internal class FusionModule : Module
{
    public override string Name => "Mash Gamemode Library";

    protected override void OnModuleRegistered()
    {
        ModuleMessageManager.RegisterHandler<RemoteEventMessageHandler>();
    }
}