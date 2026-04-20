using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using MashGamemodeLibrary.Entities.ECS.Attributes;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Player.Data;
using MashGamemodeLibrary.Player.Data.Rules.Rules;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Spectating.data.Rules;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Teams;
using UnityEngine;

namespace TheHunt.Components;

[LocalOnly]
public class HiderFinallyMarker : IComponent, IComponentPlayerReady, IComponentRemoved, IPlayerRuleChangedCallback
{
    private const string MarkerBarcode = "Mash.TheHuntAssets.Spawnable.PlayerMarker";

    private NetworkPlayer? _target;
    private Poolee? _poolee;
    private MeshRenderer? _renderer;
    
    public void OnReady(NetworkPlayer networkPlayer, MarrowEntity marrowEntity)
    {
        _target = networkPlayer;
        
        var spawnable = LocalAssetSpawner.CreateSpawnable(MarkerBarcode);
        LocalAssetSpawner.Register(spawnable);
        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, poolee =>
        {
            _poolee = poolee;
            _renderer = _poolee.GetComponentInChildren<MeshRenderer>();
            
            if (!networkPlayer.HasRig)
                return;
            
            if (networkPlayer.PlayerID.IsSpectating())
                _renderer.enabled = false;
            
            var head = networkPlayer.RigRefs.RigManager.physicsRig.m_head;
            var transform = _poolee.transform;
            transform.SetParent(head);
            transform.localPosition = Vector3.zero;
        });
    }
    
    public void OnRemoved()
    {
        if (_poolee == null) return;

        _poolee.Despawn();
        _poolee = null;
    }
    
    public void OnPlayerRuleChanged(NetworkPlayer networkPlayer, IPlayerRule newRule)
    {
        if (_renderer == null)
            return;
        
        if (networkPlayer?.PlayerID?.Equals(_target?.PlayerID) != true)
            return;
        
        if (newRule is not PlayerSpectatingRule rule)
            return;

        _renderer.enabled = !rule.IsSpectating;
    }
}