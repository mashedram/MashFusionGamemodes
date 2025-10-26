using LabFusion.SDK.Modules;
using MashGamemodeLibrary;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Player.Actions;
using MashGamemodeLibrary.Spectating;
using MashGamemodeLibrary.Vision;
using MelonLoader;
#if DEBUG
using MashGamemodeLibrary.Debug;
#endif

[assembly: MelonInfo(typeof(Mod), "Mash's Gamemode Library", "0.1.0", "Mash")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

[assembly: NetworkIdentifiable("MGL")]
namespace MashGamemodeLibrary;

public class Mod : MelonMod
{
    public override void OnInitializeMelon()
    {
        var fusionMod = FindMelon("LabFusion", "Lakatrazz");
        if (fusionMod == null) return;

        ModuleManager.RegisterModule<FusionModule>();

        RemoteEventMessageHandler.RegisterMod<Mod>();
        EntityTagManager.RegisterAll<Mod>();

        PlayerHider.Register();
    }

    public override void OnUpdate()
    {
        PlayerHider.Update();
        PlayerActionManager.Update();
#if DEBUG
        DebugKeybind.UpdateAll();
#endif
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        EntityTagManager.ClearAll();
        PlayerGrabManager.ClearOverwrites();
        PlayerHider.Reset();
        SpectatorManager.LocalReset();
    }
}