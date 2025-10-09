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
    
    public static void InvokeOnGunFired(Gun instance)
    {
        var holder = instance.triggerGrip.attachedHands._items[0];
        
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