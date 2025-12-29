using BoneStrike;
using LabFusion.SDK.Modules;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.networking.Control;
using MelonLoader;

[assembly: MelonInfo(typeof(Mod), "Bonestrike", "0.2.0", "Mash")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

[assembly: NetworkIdentifiable("bonestrike")]

namespace BoneStrike;

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