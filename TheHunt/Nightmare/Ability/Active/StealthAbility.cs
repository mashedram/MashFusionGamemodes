using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Entities.Extenders;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.Player.Data;
using MashGamemodeLibrary.Player.Data.Rules.Rules;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheHunt.Nightmare.Ability.Active;

public class ToggleStealthPacket : INetSerializable, IKnownSenderPacket
{
    private bool _isStealth;
    public bool IsStealth => _isStealth;
    public byte SenderSmallId { get; set; }

    public ToggleStealthPacket()
    {
    }
    
    public ToggleStealthPacket(bool isStealth)
    {
        _isStealth = isStealth;
    }
    
    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _isStealth);
    }
}

public class StealthAbility : IActiveAbility, IUpdatingAbility
{
    private static readonly RemoteEvent<ToggleStealthPacket> ToggleStealthEvent = new RemoteEvent<ToggleStealthPacket>(OnStealthToggleEvent);

    private GameObject? _light;
    private bool _isStealthed;
    private float _stealthTimer;
    
    public void OnAdded(NetworkPlayer networkPlayer)
    {
        // No light unless local
        if (!networkPlayer.PlayerID.IsMe)
            return;
        
        _light = networkPlayer.RigRefs.Head.CreateSafeObject("EntityLight");
        var light = _light.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = Color.red;
        light.range = 90f;
        light.spotAngle = 90f;
        light.intensity = 5f;

        light.shadows = LightShadows.Soft;

        _light.transform.localPosition = Vector3.forward * 0.5f;
        _light.transform.localRotation = Quaternion.identity;
    }
    
    public void OnRemoved(NetworkPlayer networkPlayer)
    {
        if (_light == null)
            return;
        
        Object.Destroy(_light);
        _light = null;
    }

    private void ExitStealth()
    {
        _isStealthed = false;
        _stealthTimer = 0f;
        
        ToggleStealthEvent.Call(new ToggleStealthPacket(true));
    }
    
    public void Update(float delta)
    {
        if (!_isStealthed)
            return;
        
        _stealthTimer -= delta;
        if (_stealthTimer <= 0f)
            ExitStealth();
    }

    public Handedness Handedness { get; } = Handedness.LEFT;
    public void UseAbility(NetworkPlayer networkPlayer)
    {
        _isStealthed = true;
        _stealthTimer = Cooldown / 2f;
        
        ToggleStealthEvent.Call(new ToggleStealthPacket(true));
    }
    
    public float Cooldown => 30f;
    
    private static void OnStealthToggleEvent(ToggleStealthPacket packet)
    {
        PlayerDataManager.GetPlayerData(packet.SenderSmallId)?.ModifyRule<ForceHideRule>(r => r.IsEnabled = packet.IsStealth);
    }
}