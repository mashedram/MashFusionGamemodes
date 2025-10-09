using Clockhunt.Entities.Tags;
using Clockhunt.Vision;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Marrow.Extenders;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Spectating;
using MashGamemodeLibrary.Vision;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using BodyPose = Il2CppSLZ.Marrow.BodyPose;

namespace Clockhunt.Nightmare.Implementations;

class SoundTriggerData
{
    public float Duration;
    public float Distance;
    
    public SoundTriggerData(float duration, float distance)
    {
        Duration = duration;
        Distance = distance;
    }
}

public class BlindNightmareInstance : NightmareInstance
{
    private static readonly SoundTriggerData ClockSoundTrigger = new(3f, 70);
    private static readonly SoundTriggerData GunshotSoundTrigger = new(5f, 50f);
    private const float MovementBaseSpeedSqrt = 30f;
    private static readonly SoundTriggerData WalkSoundTrigger = new(1f, 10f);
    private static readonly SoundTriggerData JumpSoundTrigger = new(1f, 15f);
    
    private Dictionary<byte, float> _visibilityTimers = new();
    
    public BlindNightmareInstance(NetworkPlayer owner, BlindNightmareDescriptor descriptor) : base(owner, descriptor)
    {
    }

    public override bool CanGrab(NetworkEntity? entity, MarrowEntity? marrowEntity)
    {
        return true;
    }

    public override void OnApplied()
    {
        Owner.RigRefs.RigManager.avatar.footstepsWalk.audioClips.Clear();
        Owner.RigRefs.RigManager.avatar.footstepsJog.audioClips.Clear();
        
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
        });
    }

    public override void OnUpdate(float delta)
    {
        foreach (var player in NetworkPlayer.Players.Where(p => !p.PlayerID.IsSpectating()))
        {
            if (player.PlayerID.Equals(Owner.PlayerID))
                continue;
            
            UpdateMovement(player);

            if (ClockGrabNotifier.Holders.Contains(player.PlayerID))
            {
                UpdateVisibilityTimers(player, ClockSoundTrigger);
            }
            
            var timer = _visibilityTimers.GetValueOrDefault(player.PlayerID, 0f);
            player.PlayerID.ForceHide(timer <= 0f);
            _visibilityTimers[player.PlayerID] = Math.Max(0f, timer - delta);
        }
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

        _visibilityTimers[player.PlayerID] = Math.Max(data.Duration * modifier, _visibilityTimers.GetValueOrDefault(player.PlayerID, 0f));
    }

    private void OnGunFired(NetworkPlayer shooter, Gun gun)
    {
        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            UpdateVisibilityTimers(shooter, GunshotSoundTrigger);
        });
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
        Vitality = 20f,
        UpperStrength = 10f,
        Speed = 4f,
        Agility = 4f,
        LowerStrength = 8f
    };

    public override NightmareInstance CreateInstance(NetworkPlayer owner)
    {
        return new BlindNightmareInstance(owner, this);
    }
}