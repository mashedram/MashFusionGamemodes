using Clockhunt.Entities.Tags;
using Clockhunt.Vision;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Extensions;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Spectating;
using MashGamemodeLibrary.Vision;
using UnityEngine;

namespace Clockhunt.Nightmare.Implementations;

internal class SoundTriggerData
{
    public float Distance;
    public float Duration;

    public SoundTriggerData(float duration, float distance)
    {
        Duration = duration;
        Distance = distance;
    }
}

public class BlindNightmareInstance : NightmareInstance
{
    private const string BlindHiderKey = "BlindHiderKey";
    private const float MovementBaseSpeedSqrt = 20f;

    private static readonly SoundTriggerData ClockSoundTrigger = new(3f, 70);
    private static readonly SoundTriggerData GunshotSoundTrigger = new(5f, 50f);
    private static readonly SoundTriggerData VoiceSoundTrigger = new(2f, 40f);
    private static readonly SoundTriggerData WalkSoundTrigger = new(1f, 30f);
    private static readonly SoundTriggerData JumpSoundTrigger = new(1f, 30f);

    private readonly Dictionary<byte, float> _visibilityTimers = new();

    public BlindNightmareInstance(byte owner, BlindNightmareDescriptor descriptor) : base(owner, descriptor)
    {
    }

    public override void OnApplied()
    {
        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            VisionManager.EnableNightVision();
            PlayerGunManager.OnGunFired += OnGunFired;
        });
    }

    public override void OnRemoved()
    {
        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            PlayerGunManager.OnGunFired -= OnGunFired;

            NetworkPlayer.Players.ForEach(player => player.PlayerID.SetHidden(BlindHiderKey, false));
        });
    }

    public override void OnUpdate(float delta)
    {
        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            foreach (var player in NetworkPlayer.Players.Where(p => !p.PlayerID.IsSpectating()))
            {
                if (player.PlayerID.Equals(Owner.PlayerID))
                    continue;

                if (!player.HasRig)
                    continue;

                UpdateMovement(player);

                var distance = Vector3.Distance(Owner.RigRefs.Head.position, player.RigRefs.Head.position);
                var distanceMod = 1.0f - Mathf.Clamp01(distance / player.VoiceSource.MaxMicrophoneDistance);
                var amplitudeMod = Mathf.Clamp01(player.VoiceSource.VoiceSource.Amplitude * 15f);
                var value = amplitudeMod * distanceMod;
                UpdateVisibilityTimers(player, VoiceSoundTrigger, value);

                if (ClockGrabNotifier.Holders.Contains(player.PlayerID))
                    UpdateVisibilityTimers(player, ClockSoundTrigger);

                var timer = _visibilityTimers.GetValueOrDefault(player.PlayerID, 0f);
                player.PlayerID.SetHidden(BlindHiderKey, timer <= 0f);
                _visibilityTimers[player.PlayerID] = Math.Max(0f, timer - delta);
            }
        });
    }

    private void UpdateMovement(NetworkPlayer player)
    {
        var rigManager = player.RigRefs.RigManager;

        // Check if we are sprinting
        if (rigManager.remapHeptaRig._jumping)
        {
            UpdateVisibilityTimers(player, JumpSoundTrigger);
            return;
        }

        var velocity = rigManager.remapHeptaRig._currentVelocity.sqrMagnitude;
        var normalizedVelocity = velocity / (rigManager.avatar?.speed ?? 1);
        // Check if we are moving at all
        if (normalizedVelocity < 0.1f)
            return;

        UpdateVisibilityTimers(player, WalkSoundTrigger, velocity / MovementBaseSpeedSqrt);
    }

    private void UpdateVisibilityTimers(NetworkPlayer player, SoundTriggerData data, float modifier = 1f)
    {
        var distance = Vector3.Distance(player.RigRefs.Head.transform.position, Owner.RigRefs.Head.transform.position);
        if (distance > data.Distance * modifier)
            return;

        _visibilityTimers[player.PlayerID] = Math.Max(data.Duration * modifier,
            _visibilityTimers.GetValueOrDefault(player.PlayerID, 0f));
    }

    private void OnGunFired(NetworkPlayer shooter, Gun gun)
    {
        Executor.RunIfMe(Owner.PlayerID, () => { UpdateVisibilityTimers(shooter, GunshotSoundTrigger); });
    }
}

public class BlindNightmareDescriptor : NightmareDescriptor
{
    public override string Name => "Blind";
    public override string HunterDescription => "You can only see movement.";
    public override string SurvivorDescription => "Stand Still";
    public override string Avatar => "Random.OWNTeamAvatars.Avatar.Entity";
    public override int Weight => 5;
    public override float AbilityCooldown => 60f;

    public override PlayerStats Stats => new()
    {
        Vitality = 50f,
        UpperStrength = 10f,
        Speed = 4f,
        Agility = 4f,
        LowerStrength = 8f
    };

    public override NightmareInstance CreateInstance(byte ownerID)
    {
        return new BlindNightmareInstance(ownerID, this);
    }
}