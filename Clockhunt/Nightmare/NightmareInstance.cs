using Clockhunt.Game;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Execution;
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