using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using MashGamemodeLibrary.Entities.ECS.Attributes;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Teams;
using UnityEngine;

namespace TheHunt.Components;

[LocalOnly]
public class HiderFinallyMarker : IComponent, IComponentPlayerReady, IComponentRemoved
{
    private const string MarkerBarcode = "Mash.TheHuntAssets.Spawnable.PlayerMarker";

    private Poolee? _poolee;
    
    public void OnReady(NetworkPlayer networkPlayer, MarrowEntity marrowEntity)
    {
        var spawnable = LocalAssetSpawner.CreateSpawnable(MarkerBarcode);
        LocalAssetSpawner.Register(spawnable);
        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, poolee =>
        {
            _poolee = poolee;
            
            if (!networkPlayer.HasRig)
                return;

            var head = networkPlayer.RigRefs.RigManager.physicsRig.m_head;
            var transform = _poolee.transform;
            transform.SetParent(head);
            transform.localPosition = Vector3.zero;
        });
    }
    
    public void OnRemoved(NetworkEntity networkEntity)
    {
        if (_poolee == null) return;

        _poolee.Despawn();
        _poolee = null;
    }
}