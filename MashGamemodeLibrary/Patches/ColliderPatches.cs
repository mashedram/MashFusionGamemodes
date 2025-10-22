using HarmonyLib;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Marrow.Extenders;
using LabFusion.MonoBehaviours;
using LabFusion.Player;
using MashGamemodeLibrary.Spectating;
using UnityEngine;

namespace MashGamemodeLibrary.Patches;

// We patch into this, because spectators are a multiplayer-only thing anyway
// And we don't have to add these manually
[HarmonyPatch(typeof(CollisionSyncer))]
public class ColliderPatches
{
    [HarmonyPatch("OnCollisionEnter")]
    [HarmonyPrefix]
    private static bool OnCollisionEnter_Prefix(Collision collision)
    {
        // If we collide but aren't spectating, we don't care.
        if (!SpectatorManager.IsLocalPlayerSpectating())
            return true;

        var rb = collision.rigidbody;
        if (!rb)
        {
            return true;
        }

        var go = rb.gameObject;
        var marrowBody = MarrowBody.Cache.Get(go);

        if (marrowBody == null)
            return true;

        if (!MarrowBodyExtender.Cache.TryGet(marrowBody, out var networkEntity))
            return true;

        if (networkEntity.ID < byte.MaxValue)
            return true;

        SpectatorManager.StartIgnoring(networkEntity);
        return false;
    }
}