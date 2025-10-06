using Clockhunt.Vision;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow;
using LabFusion.Marrow.Pool;
using LabFusion.SDK.Triggers;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Spectating;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Clockhunt.Nightmare.Implementations;

public class EntityNightmareInstance : NightmareInstance
{
    private GameObject? _light;
    
    public EntityNightmareInstance(NetworkPlayer owner, EntityNightmareDescriptor descriptor) : base(owner, descriptor)
    {
    }

    public override void OnApplied()
    {
        Executor.RunIfMe(Owner.PlayerID, VisionManager.EnableNightVision);
        
        Executor.RunIfRemote(Owner.PlayerID, () =>
        {
            _light = new GameObject("EntityLight");
            var light = _light.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = Color.red;
            light.range = 90f;
            light.spotAngle = 90f;
            light.intensity = 5f;

            light.shadows = LightShadows.Soft;
        
            _light.transform.SetParent(Owner.RigRefs.Head.transform);
            _light.transform.localPosition = Vector3.forward * 0.5f;
            _light.transform.localRotation = Quaternion.identity;
        });
    }

    public override void OnRemoved()
    {
        if (_light)
            Object.Destroy(_light);
        _light = null;
    }
    
    private void SpawnMarkerAt(NetworkPlayer player)
    {
        var spawnable = LocalAssetSpawner.CreateSpawnable("Mash.ClockhuntAssets.Spawnable.HeartMarker");
        LocalAssetSpawner.Register(spawnable);
        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, poolee =>
        {
            poolee.transform.position = player.RigRefs.Head.position;
            
            PooleeHelper.DespawnDelayed(poolee, 8f);
        });
    }

    public override void OnAbilityKeyTapped(Handedness handedness)
    {
        foreach (var player in NetworkPlayer.Players
                     .Where(e => !SpectatorManager.IsPlayerSpectating(e.PlayerID) && 
                                 NightmareManager.IsNightmare(e.PlayerID)))
        {
            SpawnMarkerAt(player);
        }
    }
}

public class EntityNightmareDescriptor : NightmareDescriptor
{
    public override string Name => "Entity";
    public override string HunterDescription => "Kill all players.";
    public override string SurvivorDescription => "Avoid the entity and escape.";
    public override string Avatar => "fa534c5a83ee4ec6bd641fec424c4142.Avatar.CharTallv4";
    public override int Weight => 10;
    public override float AbilityCooldown => 60f;

    public override PlayerStats Stats => new()
    {
        Vitality = 4f,
        UpperStrength = 8f,
        Speed = 3f,
        Agility = 2f,
        LowerStrength = 8f
    };
    
    public override NightmareInstance CreateInstance(NetworkPlayer owner)
    {
        return new EntityNightmareInstance(owner, this);
    }
}