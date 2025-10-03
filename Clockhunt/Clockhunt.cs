using Clockhunt.Audio;
using Clockhunt.Audio.Hunt;
using Clockhunt.Config;
using Clockhunt.Entities;
using Clockhunt.Entities.Tags;
using Clockhunt.Game;
using Clockhunt.Nightmare;
using Clockhunt.Nightmare.Implementations;
using Clockhunt.Phase;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Menu.Data;
using LabFusion.Network;
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
using MashGamemodeLibrary.Spectating;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace Clockhunt;

public class Clockhunt : GamemodeWithContext<ClockhuntContext>
{
    public override string Title => "Clockhunt";
    public override string Author => "Mash";

    public override bool AutoHolsterOnDeath { get; }

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

    public override void OnGamemodeStarted()
    {
        Context.PhaseManager.ResetPhases();
        Context.EnvironmentPlayer.StartPlaying(new EnvironmentProfile<ClockhuntMusicContext>("night", new EnvironmentState<ClockhuntMusicContext>[]
        {
            new ChaseEnvironmentState(),
            new HuntEndEnvironmentState(),
            new HuntMiddlePhaseEnvironmentState(),
            new HuntStartPhaseEnvironmentState(),
            new HidePhaseEnvironmentState(),
        }, LocalWeatherManager.ClearLocalWeather));
        
        WinStateManager.SetLives(3, false);
        HuntPhase.SetDeliveryPosition(Context.LocalPlayer.RigRefs.Head.position);
    }

    public override void OnGamemodeStopped()
    {
        Context.EnvironmentPlayer.Stop();
        Context.ClockAudioPlayer.StopPlaying();
        Context.EscapeAudioPlayer.StopAll();
        
        if (!NetworkInfo.IsHost)
            return;
        
        NightmareManager.ClearNightmares();
        ClockManager.ClearClocks();
        SpectatorManager.Clear();
    }
}