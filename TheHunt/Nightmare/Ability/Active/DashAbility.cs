using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using UnityEngine;

namespace TheHunt.Nightmare.Ability.Active;

public class DashAbility : IActiveAbility, INetSerializable
{
    private float _cooldown;
    private Handedness _handedness;
    public Handedness Handedness => _handedness;

    public string Description => "Dash at high speeds";
    
    public DashAbility()
    {
        _handedness = Handedness.RIGHT;
        _cooldown = 10f;
    }
    
    public DashAbility(Handedness handedness, float cooldown)
    {
        _handedness = handedness;
        _cooldown = cooldown;
    }

    public void UseAbility(Nightmare nightmare, NetworkPlayer networkPlayer)
    {
        if (!networkPlayer.HasRig)
            return;

        var physRig = networkPlayer.RigRefs.RigManager.physicsRig;
        var feet = physRig._feetRb;
        
        var forward = networkPlayer.RigRefs.Head.forward;
        forward.Normalize();
        
        feet.AddForce(forward * 250f, ForceMode.VelocityChange);
    }
    
    public float Cooldown => _cooldown;
    
    public void OnAdded(NetworkPlayer networkPlayer)
    {
    }
    public void OnRemoved(NetworkPlayer networkPlayer)
    {
    }
    
    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _handedness);
        serializer.SerializeValue(ref _cooldown);
    }
}