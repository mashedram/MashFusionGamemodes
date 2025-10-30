using Il2CppSLZ.Marrow;
using LabFusion.Entities;

namespace MashGamemodeLibrary.Entities.Interaction;

public static class PlayerGunManager
{
    public delegate void OnGunFiredHandler(NetworkPlayer shooter, Gun gun);

    public static event OnGunFiredHandler? OnGunFired;

    // TODO: Check if this works and make it configurable
    public static void NormalizeGunDamage(Gun gun)
    {
        gun.defaultCartridge.projectile.damageMultiplier = 1f;
    }

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