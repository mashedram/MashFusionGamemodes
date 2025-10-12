using Clockhunt.Game;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player;
using MelonLoader;
using UnityEngine;

namespace Clockhunt.Nightmare;

public class NightmareInstance
{
    private static string NightmareGrabKey => "Nightmare_Grab";

    private byte _smallOwnerID;
    private NetworkPlayer? _owner;
    public NetworkPlayer Owner => FetchOwner();
    public NightmareDescriptor Descriptor { get; }
    protected float AbilityTimer { get; set; }
    protected bool IsAbilityOnCooldown { get; private set; }
    

    protected NightmareInstance(byte smallId, NightmareDescriptor descriptor)
    {
        var owner = NetworkPlayerManager.TryGetPlayer(smallId, out var player) ? player : null;
        
        _smallOwnerID = smallId;
        _owner = owner;
        Descriptor = descriptor;
        AbilityTimer = 0f;
        IsAbilityOnCooldown = false;
    }
    
    private bool IsOwnerValid()
    {
        return _owner != null && _owner.PlayerID.IsValid;
    }

    private NetworkPlayer FetchOwner()
    {
        if (IsOwnerValid())
        {
            return _owner!;
        }
        
        if (NetworkPlayerManager.TryGetPlayer(_smallOwnerID, out var player))
        {
            _owner = player;
            return _owner;
        }
        
        MelonLogger.Error($"Unable to find the owner of the nightmare with playerid: {_smallOwnerID}");
        return null!;
    }
    
    public virtual bool CanStartChaseMusic(NetworkPlayer nightmare, float distance, bool lineOfSight)
    {
        return distance < 50f && lineOfSight;
    }

    public virtual bool CanStartTensionMusic(NetworkPlayer nightmare, float distance, bool lineOfSight)
    {
        return distance < 25f && lineOfSight;
    }
    
    public virtual bool CanGrab(NetworkEntity? entity, MarrowEntity? marrowEntity)
    {
        return marrowEntity && NetworkPlayerManager.TryGetPlayer(marrowEntity, out _);
    }
 
    public virtual void OnApplied()
    {
    }

    /**
     * When removed from another player (host only)
     */
    public virtual void OnRemoved()
    {

    }

    public virtual void OnUpdate(float delta)
    {
        
    }

    public virtual void OnAbilityKeyTapped(Handedness handedness)
    {
        
    }

    public virtual void OnPlayerAction(PlayerID playerID, PhaseAction action, Handedness handedness)
    {
        
    }
    
    // Local Methods
    protected T GetDescriptorAs<T>() where T : NightmareDescriptor
    {
        return (T)Descriptor;
    }
    
    public void Update(float delta)
    {
        try
        {
            OnUpdate(delta);
        }
        catch (Exception error)
        {
            MelonLogger.Error($"Failed to execute player update. {error}");
        }
        
        if (!Owner.PlayerID.IsMe) return;
        if (AbilityTimer < 0f) return;
        AbilityTimer -= delta;

        if (!IsAbilityOnCooldown || !(AbilityTimer <= 0f)) return;
        IsAbilityOnCooldown = false;
        
        Notifier.Send(new Notification
        {
            Title = "Ability Ready",
            Message = $"{Descriptor.Name}'s ability is ready to use.",
            PopupLength = 3f,
            Type = NotificationType.INFORMATION,
            SaveToMenu = false,
            ShowPopup = true
        });
    }
    
    public void ResetAbilityTimer()
    {
        AbilityTimer = Descriptor.AbilityCooldown;
        IsAbilityOnCooldown = true;
    }
    
    public bool IsAbilityReady()
    {
        return AbilityTimer <= 0f;
    }

    public void Apply()
    {
        OnApplied();

        if (!Owner.PlayerID.IsMe) return;
        
        if (Descriptor.Avatar != null)
            LocalAvatar.AvatarOverride = Descriptor.Avatar;
            
        WinStateManager.SetLocalTeam(GameTeam.Nightmares);
            
        PlayerStatManager.SetStats(Descriptor.GetStats());
        
        PlayerGrabManager.SetOverwrite(NightmareGrabKey, CanGrab);
    }

    public void Remove()
    {
        OnRemoved();

        if (!Owner.PlayerID.IsMe) return;
        PlayerStatManager.ResetStats();
        PlayerGrabManager.SetOverwrite(NightmareGrabKey, null);
    }
}