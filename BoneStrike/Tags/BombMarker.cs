using LabFusion.RPC;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Interaction.Components;
using MashGamemodeLibrary.Entities.Tagging.Base;

namespace BoneStrike.Tags;

public class BombMarker : IEntityGrabCallback, IEntityTag
{
    public void OnGrab(GrabData grab)
    {
        if (!grab.IsHoldingItem(out var item)) return;
        if (!item.IsNetworked(out var entity)) return;

        NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo
        {
            EntityID = entity.ID,
            DespawnEffect = true
        });
    }
}