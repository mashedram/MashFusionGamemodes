
using LabFusion.SDK.Modules;
using MashGamemodeLibrary.networking.Control;
using MelonLoader;
using TheHunt;

[assembly: MelonInfo(typeof(Mod), "The Hunt", "0.1.0", "Mash")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

[assembly: NetworkIdentifiable("thehunt")]
namespace TheHunt;

public class Mod : MelonMod
{
    public override void OnInitializeMelon()
    {
        var fusionMod = FindMelon("LabFusion", "Lakatrazz");
        if (fusionMod == null) return;
        ModuleManager.RegisterModule<FusionModule>();

        MashGamemodeLibrary.Mod.Register<Mod>();
    }
}