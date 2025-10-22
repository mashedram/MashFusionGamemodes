using Clockhunt;
using LabFusion.SDK.Modules;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.networking.Control;
using MelonLoader;

[assembly: MelonInfo(typeof(Mod), "Clockhunt", "0.1.0", "Mash")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

[assembly: NetworkIdentifiable("clockhunt")]
namespace Clockhunt;

public class Mod : MelonMod
{
    public override void OnInitializeMelon()
    {
        var fusionMod = FindMelon("LabFusion", "Lakatrazz");
        if (fusionMod == null) return;
        ModuleManager.RegisterModule<FusionModule>();

        RemoteEventMessageHandler.RegisterMod<Mod>();
    }
}