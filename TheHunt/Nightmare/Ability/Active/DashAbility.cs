using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using UnityEngine;

namespace TheHunt.Nightmare.Ability.Active;

public class DashAbility : IActiveAbility
{
    public Handedness Handedness => Handedness.RIGHT;
    public void UseAbility(NetworkPlayer networkPlayer)
    {
        if (!networkPlayer.HasRig)
            return;

        var physRig = networkPlayer.RigRefs.RigManager.physicsRig;
        var feet = physRig._feetRb;
        
        var forward = networkPlayer.RigRefs.Head.forward;
        forward.Normalize();
        
        feet.AddForce(forward * 250f, ForceMode.VelocityChange);
    }
    
    public float Cooldown => 8f;
    
    public void OnAdded(NetworkPlayer networkPlayer)
    {
    }
    public void OnRemoved(NetworkPlayer networkPlayer)
    {
    }
}