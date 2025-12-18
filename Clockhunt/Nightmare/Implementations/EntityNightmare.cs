using Clockhunt.Nightmare.Config;
using Clockhunt.Vision;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Marrow.Pool;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Modifiers;
using MashGamemodeLibrary.Audio.Players.Callers;
using MashGamemodeLibrary.Audio.Players.Object;
using MashGamemodeLibrary.Entities.Extenders;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Player.Spectating;
using MashGamemodeLibrary.Util;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Clockhunt.Nightmare.Implementations;

internal class RoarRequest : IParameterPacket<Vector3>
{
    public Vector3 RoarPosition;

    public Vector3 Value
    {
        get => RoarPosition;
        init => RoarPosition = value;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref RoarPosition);
    }
}

public class EntityNightmareInstance : NightmareInstance
{
    private static readonly ClientCallableAudioPlayer<Vector3, RoarRequest> RoarAudioPlayer = new(new PositionalAudioPlayer("EntityRoar",
        new SyncedAudioContainer(new AudioFileLoader("Growls")),
        new AudioModifierFactory().AddModifier<AudioSettingsModifier>(modifier =>
            modifier.SetMaxDistance(160f).SetCustomRolloff(AnimationCurve.Linear(0f, 1f, 1f, 0.2f)))));
    
    private GameObject? _light;

    public EntityNightmareInstance(byte owner, NightmareDescriptor descriptor) : base(owner, descriptor)
    {
    }

    public override void OnApplied()
    {
        Executor.RunIfMe(Owner.PlayerID, VisionManager.EnableNightVision);

        Executor.RunIfRemote(Owner.PlayerID, () =>
        {
            _light = Owner.RigRefs.Head.CreateSafeObject("EntityLight");
            var light = _light.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = Color.red;
            light.range = 90f;
            light.spotAngle = 90f;
            light.intensity = 5f;

            light.shadows = LightShadows.Soft;

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
        var player = NetworkPlayer.Players
            .Where(e => !SpectatorManager.IsSpectating(e.PlayerID) && !NightmareManager.IsNightmare(e.PlayerID))
            .DefaultIfEmpty(null)
            .GetRandom();
        
        if (player != null) 
            SpawnMarkerAt(player);

        RoarAudioPlayer.PlayRandom(Owner.RigRefs.Head.position);
    }
}

public class EntityNightmareDescriptor : NightmareDescriptor
{
    public override string Name => "Entity";
    public override string HunterDescription => "Tap menu to locate all survivors.";
    public override string SurvivorDescription => "Avoid the entity and escape.";
    public override string Avatar => "fa534c5a83ee4ec6bd641fec424c4142.Avatar.CharTallv4";

    public override PlayerStats Stats => new()
    {
        Vitality = 3f,
        UpperStrength = 8f,
        Speed = 3f,
        Agility = 2f,
        LowerStrength = 8f
    };

    // We want this to be the only one enabled by default
    public EntityNightmareDescriptor()
    {
        Enabled = true;
    }

    public override NightmareInstance CreateInstance(byte ownerID)
    {
        return new EntityNightmareInstance(ownerID, this);
    }
}