using System.Diagnostics.CodeAnalysis;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Marrow.Extenders;
using LabFusion.Network;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace MashGamemodeLibrary.Entities.Interaction.Grabbing;

public static class MarrowInventoryExtender
{
    public static bool TryGetManager(this Hand hand, [MaybeNullWhen(false)] out RigManager rigManager)
    {
        rigManager = hand.manager;
        return rigManager != null;
    }
    
    public static bool TryGetNetworkPlayer(this Hand hand, [MaybeNullWhen(false)] out NetworkPlayer networkPlayer)
    {
        networkPlayer = null;
        if (!hand.TryGetManager(out var rigManager)) return false;
        return NetworkPlayerManager.TryGetPlayer(rigManager, out networkPlayer);
    }
    
    public static NetworkEntity? GetNetworkEntity(this MarrowEntity entity)
    {
        if (entity == null) return null;
        return IMarrowEntityExtender.Cache.Get(entity);
    }
    
    public static bool TryGetNetworkedEntity(this MarrowEntity entity, [MaybeNullWhen(false)] out NetworkEntity networkEntity)
    {
        networkEntity = entity.GetNetworkEntity();
        return networkEntity != null;
    }
    
    public static bool TryGetHeldEntity(this Hand hand, [MaybeNullWhen(false)] out NetworkEntity networkEntity)
    {
        networkEntity = null;
        if (!hand.HasAttachedObject())
            return false;

        var host = hand.AttachedReceiver.Host.TryCast<InteractableHost>();
        if (host == null)
            return false;
        
        return InteractableHostExtender.Cache.TryGet(host, out networkEntity);
    }
    
    // Components

    public static bool IsHolding<TComponent>(this Hand hand) where TComponent : class, IComponent
    {
        if (!hand.HasAttachedObject())
            return false;

        var host = hand.AttachedReceiver.Host.TryCast<InteractableHost>();
        if (host == null)
            return false;
        
        if (!InteractableHostExtender.Cache.TryGet(host, out var entity))
            return false;

        return entity.GetComponent<TComponent>() != null;
    }

    public static bool IsHolding<TComponent>(this NetworkPlayer player, Handedness handedness) where TComponent : class, IComponent
    {
        return player.HasRig && player.RigRefs.GetHand(handedness).IsHolding<TComponent>();
    }
    
    // Fetching
    
    public static IEnumerable<Hand> GetHandsHolding<T>(this NetworkPlayer player) where T : class, IComponent
    {
        if (!player.HasRig)
            return Array.Empty<Hand>();
        
        return player.RigRefs.GetHandsHoldingTag<T>();
    }
    
    public static IEnumerable<Hand> GetHandsHoldingTag<T>(this RigRefs rigRefs) where T : class, IComponent
    {
        var hands = new[]
        {
            rigRefs.LeftHand,
            rigRefs.RightHand
        };

        return hands
            .Where(hand =>
            {
                if (!hand.TryGetHeldEntity(out var entity))
                    return false;
                
                return entity.GetComponent<T>() != null;
            });
    }
}