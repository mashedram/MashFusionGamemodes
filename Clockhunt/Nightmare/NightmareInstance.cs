using Clockhunt.Game;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Player;
using UnityEngine;

namespace Clockhunt.Nightmare;

public class NightmareInstance
{
    private static string NightmareGrabKey => "Nightmare_Grab";
    
    public NetworkPlayer Owner { get; }
    public NightmareDescriptor Descriptor { get; }
    protected float AbilityTimer { get; set; }
    

    protected NightmareInstance(NetworkPlayer owner, NightmareDescriptor descriptor)
    {
        Owner = owner;
        Descriptor = descriptor;
    }
    
    public virtual bool CanStartChaseMusic(NetworkPlayer nightmare, float distance, bool lineOfSight)
    {
        return distance < 50f && lineOfSight;
    }

    public virtual bool CanStartTensionMusic(NetworkPlayer nightmare, float distance, bool lineOfSight)
    {
        return distance < 25f && lineOfSight;
    }
    
    public virtual bool CanGrab(NetworkEntity entity, MarrowEntity marrowEntity)
    {
        return NetworkPlayerManager.TryGetPlayer(marrowEntity, out _);
    }
    
    /**
     * When applied to another player (host only)
     */
    public virtual void OnApplied()
    {
        PlayerGrabManager.SetOverwrite(NightmareGrabKey, CanGrab);
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
    
    // Local Methods
    protected T GetDescriptorAs<T>() where T : NightmareDescriptor
    {
        return (T)Descriptor;
    }
    
    public void Update(NetworkPlayer player, float delta)
    {
        OnUpdate(delta);
        
        if (!player.PlayerID.IsMe) return;
        AbilityTimer -= delta;
    }
    
    public void ResetAbilityTimer()
    {
        AbilityTimer = Descriptor.AbilityCooldown;
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
            
        PlayerStatManager.SetStats(Descriptor.Stats);
    }

    public void Remove()
    {
        OnRemoved();

        if (!Owner.PlayerID.IsMe) return;
        PlayerStatManager.ResetStats();
        LocalAvatar.AvatarOverride = null;
    }
}