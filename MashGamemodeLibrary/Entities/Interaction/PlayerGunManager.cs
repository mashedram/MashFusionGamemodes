using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Marrow.Extenders;
using LabFusion.Network;
using MashGamemodeLibrary.Entities.Interaction.Components;
using MashGamemodeLibrary.Entities.Tagging;

namespace MashGamemodeLibrary.Entities.Interaction;

public static class PlayerGunManager
{
    public delegate void OnGunFiredHandler(NetworkPlayer shooter, Gun gun);
    
    public static event OnGunFiredHandler? OnGunFired;
    
    public static void InvokeGunFired(Gun instance)
    {
        var triggerGrip = instance.triggerGrip;
        if (triggerGrip == null)
            return;

        var attachedHands = triggerGrip.attachedHands;
        if (attachedHands.Count == 0) return;
        
        var holder = attachedHands._items[0];
        
        if (!holder)
            return;

        var rigManager = holder.manager;
        if (!rigManager)
            return;
        
        if (!NetworkPlayerManager.TryGetPlayer(rigManager, out var player))
            return;
        
        OnGunFired?.Invoke(player, instance);
    }
}