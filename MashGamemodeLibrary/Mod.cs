using System.Reflection;
using System.Runtime.CompilerServices;
using BoneLib;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;
using MashGamemodeLibrary;
using MashGamemodeLibrary.Audio.Music;
using MashGamemodeLibrary.Audio.Registry;
using MashGamemodeLibrary.Config;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.ECS.Caches;
using MashGamemodeLibrary.Entities.ECS.Networking;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.networking.Compatiblity;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Actions;
using MashGamemodeLibrary.Player.Collision;
using MashGamemodeLibrary.Player.Spectating;
using MashGamemodeLibrary.Player.Team;
using MashGamemodeLibrary.Player.Visibility;
using MashGamemodeLibrary.Util;
using MashGamemodeLibrary.Vision;
using MelonLoader;
using MelonLoader.Utils;
#if DEBUG
using MashGamemodeLibrary.Debug;
#endif

[assembly: MelonInfo(typeof(Mod), "Mash's Gamemode Library", "0.1.0", "Mash")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

[assembly: NetworkIdentifiable("MGL")]

namespace MashGamemodeLibrary;

public class Mod : MelonMod
{
    public static readonly string ModDataDirectory = MelonEnvironment.UserDataDirectory + "/mashgamemodelibrary";
    
    public override void OnInitializeMelon()
    {
        var fusionMod = FindMelon("LabFusion", "Lakatrazz");
        if (fusionMod == null) return;
	    
        ModuleManager.RegisterModule<FusionModule>();

        RegisterInternal<Mod>();
        

        PlayerHider.Register();
        NetworkEventsExtender.Register();

        MultiplayerHooking.OnDisconnected += Cleanup;
        Hooking.OnWarehouseReady += OnWarehouseReady;
    }

    public override void OnUpdate()
    {
        PlayerHider.Update();
        PlayerActionManager.Update();
        ConfigManager.Update();
        SpawnHelper.Update();
#if DEBUG
        DebugKeybind.UpdateAll();
#endif
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        Cleanup();
    }

    private void OnWarehouseReady()
    {
        AudioRegistry.RegisterAll();
    }

    private static void Cleanup()
    {
        LocalEcsCache.Clear();
        PlayerGrabManager.Reset();
        PlayerHider.Reset();
        SpectatorManager.LocalReset();
        PlayerGunManager.Reset();
        GamemodeCompatibilityChecker.ClearRemoteHashes();
    }

    private static void RegisterInternal<T>()
    {
        EcsManager.RegisterAll<T>();
        RemoteEventMessageHandler.RegisterMod<T>();
        AutoRegistery.Register<T>();
    }
    
    public static void Register<T>()
    {
        GamePhaseManager.Registry.RegisterAll<T>();
        TeamManager.Registry.RegisterAll<T>();
        RegisterInternal<T>();
    }
}