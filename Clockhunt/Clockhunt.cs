using Clockhunt.Audio;
using Clockhunt.Audio.Hunt;
using Clockhunt.Config;
using Clockhunt.Entities;
using Clockhunt.Game;
using Clockhunt.Nightmare;
using Clockhunt.Phase;
using Clockhunt.Vision;
using LabFusion.Menu.Data;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.Effector.Weather;
using MashGamemodeLibrary.Environment.State;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Spectating;
using MashGamemodeLibrary.Vision;
using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace Clockhunt;

internal class Clockhunt : GamemodeWithContext<ClockhuntContext>
{
    private const string CalibrationAvatar = "c3534c5a-94b2-40a4-912a-24a8506f6c79";

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

    private static void ListenToAvatarChange(Avatar avatar, string barcode)
    {
        if (barcode == CalibrationAvatar)
            return;

        LocalAvatar.AvatarOverride = barcode;
        LocalAvatar.OnAvatarChanged -= ListenToAvatarChange;
    }

    protected override void OnStart()
    {
        Executor.RunIfHost(() =>
        {
            NightmareManager.ClearNightmares();
            WinStateManager.OverwriteLives(3);
            SpectatorManager.Clear();
        });

        ClockhuntMusicContext.Reset();
        Context.EnvironmentPlayer.StartPlaying(new EnvironmentProfile<ClockhuntMusicContext>("night",
            new EnvironmentState<ClockhuntMusicContext>[]
            {
                new ChaseEnvironmentState(),
                new HuntEndEnvironmentState(),
                new HuntMiddlePhaseEnvironmentState(),
                new HuntStartPhaseEnvironmentState(),
                new HidePhaseEnvironmentState()
            }, LocalWeatherManager.ClearLocalWeather));

        NightmareManager.ClearNightmares();
        MarkerManager.ClearMarker();

        GamePhaseManager.Enable(new GamePhase[]
            { new HidePhase(), new HuntPhase(), new EscapePhase() });

        PlayerHider.HideAllSpecials();

        // TODO: Make it so that once the avatar is loader, it enforces it further
        var currentAvatar = LocalAvatar.AvatarBarcode;
        if (currentAvatar != CalibrationAvatar)
            LocalAvatar.AvatarOverride = currentAvatar;
        else
            LocalAvatar.OnAvatarChanged += ListenToAvatarChange;
    }

    protected override void OnUpdate(float delta)
    {

        
        NightmareManager.Update(delta);
        GamePhaseManager.Update(delta);
    }

    public override void OnGamemodeStopped()
    {
        base.OnGamemodeStopped();

        Context.EnvironmentPlayer.Stop();
        
        MarkerManager.ClearMarker();
        VisionManager.DisableNightVision();

        LocalAvatar.AvatarOverride = null;

        Executor.RunIfHost(() =>
        {
            Context.ClockAudioPlayer.StopPlaying();
            Context.EscapeAudioPlayer.StopAll();
            NightmareManager.ClearNightmares();
            ClockManager.ClearClocks();
        });
    }
}