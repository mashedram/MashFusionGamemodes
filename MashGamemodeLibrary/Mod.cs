using LabFusion.SDK.Modules;
using MashGamemodeLibrary;
using MashGamemodeLibrary.Debug;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.Spectating;
using MelonLoader;

[assembly: MelonInfo(typeof(Mod), "Mash's Gamemode Library", "0.1.0", "Mash")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]
namespace MashGamemodeLibrary;

public class Mod : MelonMod
{
    public override void OnInitializeMelon()
    {
        var fusionMod = FindMelon("LabFusion", "Lakatrazz");
        if (fusionMod == null) return;
        ModuleManager.RegisterModule<FusionModule>();

        SpectatorManager.Register();
    }
    
#if DEBUG
    public override void OnUpdate()
    {
        DebugKeybind.UpdateAll();
    }
#endif
}