using System.Diagnostics.CodeAnalysis;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Scene;

namespace MashGamemodeLibrary.Entities.Interaction.Grabbing;

public class GrabRequest
{
    // Holder data
    
    public Hand Hand { get; }
    public bool IsValid { get; }
    public NetworkPlayer NetworkPlayer { get; }

    // Target Data

    public Grip Grip { get; }
    public MarrowEntity GrabbedEntity => Grip._marrowEntity;
    public InteractableHost? GrabbedHost => Grip.Host.TryCast<InteractableHost>();
    public NetworkEntity? GrabbedNetworkEntity => GrabbedEntity.GetNetworkEntity();

    public GrabRequest(Hand hand, Grip grip)
    {
        Hand = hand;
        Grip = grip;
        
        if (!NetworkSceneManager.IsLevelNetworked || !hand.TryGetNetworkPlayer(out var networkPlayer)) 
            return;
        
        IsValid = true;
        NetworkPlayer = networkPlayer;
    }

    public GrabRequest(InventoryHand hand, Grip grip) : this(hand._hand, grip)
    {
    }
}