using LabFusion.SDK.Modules;
using MashGamemodeLibrary;
using MashGamemodeLibrary.networking;
using MelonLoader;

[assembly: MelonInfo(typeof(Mod), "Mash's Gamemode Library", "0.1.0", "Mash")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]
namespace MashGamemodeLibrary;

public class Mod : MelonMod
{
    public override void OnInitializeMelon()
    {
        var harmony = new HarmonyLib.Harmony("MashGamemodeLibrary");
        harmony.PatchAll();
        
        var fusionMod = FindMelon("LabFusion", "Lakatrazz");
        if (fusionMod == null) return;
        ModuleManager.RegisterModule<FusionModule>();
    }
}