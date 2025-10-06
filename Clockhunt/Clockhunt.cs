using Clockhunt.Audio;
using Clockhunt.Audio.Hunt;
using Clockhunt.Config;
using Clockhunt.Entities;
using Clockhunt.Entities.Tags;
using Clockhunt.Game;
using Clockhunt.Nightmare;
using Clockhunt.Nightmare.Implementations;
using Clockhunt.Phase;
using Clockhunt.Vision;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using LabFusion.Menu.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Object;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.Effector.Weather;
using MashGamemodeLibrary.Environment.State;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Spectating;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace Clockhunt;

public class Clockhunt : GamemodeWithContext<ClockhuntContext>
{
    public override string Title => "Clockhunt";
    public override string Author => "Mash";

    public override bool AutoHolsterOnDeath => true;
    public override bool DisableDevTools => ClockhuntConfig.DevToolsDisabled;
    public override bool DisableSpawnGun => ClockhuntConfig.DevToolsDisabled;
    public override bool DisableManualUnragdoll => ClockhuntConfig.DevToolsDisabled;


    public override void OnGamemodeRegistered()
    {
        base.OnGamemodeRegistered();
        
        EntityTagManager.RegisterAll<Mod>();
        NightmareManager.RegisterAll<Mod>();
    }

    public override GroupElementData CreateSettingsGroup()
    {
        return ClockhuntConfigMenu.CreateSettingsGroup();
    }

    private void OnStart()
    {
        ClockhuntMusicContext.Reset();
        Context.EnvironmentPlayer.StartPlaying(new EnvironmentProfile<ClockhuntMusicContext>("night", new EnvironmentState<ClockhuntMusicContext>[]
        {
            new ChaseEnvironmentState(),
            new HuntEndEnvironmentState(),
            new HuntMiddlePhaseEnvironmentState(),
            new HuntStartPhaseEnvironmentState(),
            new HidePhaseEnvironmentState(),
        }, LocalWeatherManager.ClearLocalWeather));
        
        Context.PhaseManager.Enable();

        LocalAvatar.AvatarOverride = LocalAvatar.AvatarBarcode;
        SpectatorManager.Enable();
        LocalControls.DisableSlowMo = true;
    }

    public override void OnGamemodeStarted()
    {
        OnStart();
        
        Executor.RunIfHost(() =>
        {
            NightmareManager.ClearNightmares();
            WinStateManager.SetLives(3, false);
        });
    }

    public override void OnLevelReady()
    {
        if (!IsStarted)
            return;

        OnStart();
    }

    public override void OnGamemodeStopped()
    {
        Context.EnvironmentPlayer.Stop();
        Context.ClockAudioPlayer.StopPlaying();
        Context.EscapeAudioPlayer.StopAll();
        
        Context.PhaseManager.Disable();
        
        GamemodeHelper.ResetSpawnPoints();
        MarkerManager.ClearMarker();
        VisionManager.DisableNightVision();
        
        SpectatorManager.Disable();
        LocalAvatar.AvatarOverride = null;
        LocalControls.DisableSlowMo = false;
        
        Executor.RunIfHost(() =>
        {
            NightmareManager.ClearNightmares();
            ClockManager.ClearClocks();
        });
    }
}