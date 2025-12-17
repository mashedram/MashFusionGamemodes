using System.Diagnostics;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Patches;
using MashGamemodeLibrary.Player.Collision;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Vision;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MashGamemodeLibrary.Player.Spectating;

public static class SpectatorManager
{
    private const string SpectatorHideKey = "spectatorhidekey";

    private const string GrabOverwriteKey = "spectating";

    private static GameObject? _visualEffectObject;

    private static bool _isLocalSpectating;

    private static readonly SyncedSet<byte> SpectatingPlayerIds = new("spectatingPlayerIds", new ByteEncoder());

    private static readonly HashSet<byte> HiddenPlayerIds = new();

    static SpectatorManager()
    {
        SpectatingPlayerIds.OnValueAdded += _ => Refresh();
        SpectatingPlayerIds.OnValueRemoved += _ => Refresh();

        NetworkPlayer.OnNetworkRigCreated += (player, _) => { RefreshPlayer(player); };
    }

    private static void SetMute(NetworkPlayer player, bool muted)
    {
        var audioSource = player.VoiceSource?.VoiceSource.AudioSource;
        if (audioSource) audioSource!.mute = muted;
    }

    private static GameObject GetVisualEffectObject()
    {
        if (_visualEffectObject != null) return _visualEffectObject;

        _visualEffectObject = new GameObject("SpectatorEffect");

        var volume = _visualEffectObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10;
        volume.weight = 1f;

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        volume.sharedProfile = profile;

        var colorAdjustments = profile.Add<ColorAdjustments>(true);
        colorAdjustments.saturation.value = -100f;

        return _visualEffectObject;
    }

    private static void ToggleVisualEffect(bool show)
    {
        GetVisualEffectObject().SetActive(show);
    }

    public static bool IsLocalPlayerSpectating()
    {
        return _isLocalSpectating;
    }

    public static bool IsPlayerSpectating(byte smallId)
    {
        return SpectatingPlayerIds.Contains(smallId);
    }

    public static bool IsPlayerHidden(byte smallId)
    {
        return HiddenPlayerIds.Contains(smallId);
    }

    private static bool ShouldBeSpectating(NetworkPlayer player)
    {
        var isSpectating = SpectatingPlayerIds.Contains(player.PlayerID);
        var shouldBeHidden = isSpectating && (!_isLocalSpectating || player.PlayerID.IsMe);

        return shouldBeHidden;
    }

    /// <summary>
    /// Set wether the local player can interact or not
    /// </summary>
    /// <param name="state">True if the player can interact</param>
    private static void SetLocalInteractions(bool state)
    {
        var rig = BoneLib.Player.RigManager;
        if (!state && rig)
        {
            Loadout.Loadout.ClearPlayerLoadout(rig);
        }
        
        LocalControls.DisableInteraction = !state;
        LocalControls.DisableInventory = !state;
        LocalControls.DisableAmmoPouch = !state;
        
        ToggleVisualEffect(!state);
        DevToolsPatches.CanSpawn = state;
        PlayerGrabManager.SetOverwrite(GrabOverwriteKey, state ? null : _ => false);
    }

    private static void Hide(byte smallID)
    {
        if (!HiddenPlayerIds.Add(smallID)) return;
        if (!NetworkPlayerManager.TryGetPlayer(smallID, out var player)) return;

        var playerID = player.PlayerID;

        if (player.HasRig)
        {
            player.RigRefs.LeftHand.DetachObject();
            player.RigRefs.RightHand.DetachObject();
        }

        Executor.RunIfRemote(playerID, () =>
        {
            player.PlayerID.SetHidden(SpectatorHideKey, true);
            SetMute(player, true);
        });

        PlayerColliderManager.SetColliders(player, false);

        if (!playerID.IsMe) return;

        SetLocalInteractions(false);
    }

    private static void Show(byte smallID)
    {
        if (!HiddenPlayerIds.Remove(smallID)) return;
        if (!NetworkPlayerManager.TryGetPlayer(smallID, out var player)) return;

        var playerID = player.PlayerID;

        Executor.RunIfRemote(playerID, () =>
        {
            player.PlayerID.SetHidden(SpectatorHideKey, false);
            SetMute(player, false);
        });

        PlayerColliderManager.SetColliders(player, true);

        if (!playerID.IsMe) return;
        
        SetLocalInteractions(true);
    }

    private static void RefreshPlayer(NetworkPlayer player)
    {
        if (!player.HasRig)
            return;

        var shouldBeHidden = ShouldBeSpectating(player);

        PlayerColliderManager.RefreshPlayer(player);

        if (player.PlayerID.IsMe)
        {
            PlayerStatManager.RefreshVitality();
        }

        if (shouldBeHidden)
            Hide(player.PlayerID);
        else
            Show(player.PlayerID);
    }

    private static void Refresh()
    {
        _isLocalSpectating = IsPlayerSpectating(PlayerIDManager.LocalSmallID);
        foreach (var player in NetworkPlayer.Players) RefreshPlayer(player);
    }

    public static void SetSpectating(this PlayerID playerID, bool spectating)
    {
        if (!NetworkInfo.IsHost)
        {
            MelonLogger.Error("Only the host can set spectating states!", new StackTrace());
            return;
        }

        if (spectating)
            SpectatingPlayerIds.Add(playerID);
        else
            SpectatingPlayerIds.Remove(playerID);
    }

    public static void Clear()
    {
        SpectatingPlayerIds.Clear();
    }

    public static void LocalReset()
    {
        HiddenPlayerIds.Clear();
        PlayerColliderManager.Clear();
        
        SetLocalInteractions(true);
    }
}