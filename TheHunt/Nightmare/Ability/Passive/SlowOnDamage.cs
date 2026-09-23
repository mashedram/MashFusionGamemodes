using Il2CppSLZ.Marrow.Combat;
using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Player.Speed;
using TheHunt.Teams;
using TheHunt.Utils;

namespace TheHunt.Nightmare.Ability.Passive;

public class SlowOnDamage : IAbility, IUpdate, IPlayerTakeDamageCallback, INetSerializable
{
    private float _maxSpeed;
    private float _speedPenaltyPerShot;
    private float _speedPenaltyDuration;
    private float _speedPenaltyHealDelay;
    private float _minimumSpeed;
    
    private NetworkPlayer? _networkPlayer;
    
    // Speed penalty
    private float _speedModifier = 1f;
    private float _speedHealDelay = 0f;
    
    public float SpeedModifier => _speedModifier;
    
    public SlowOnDamage() { }
    public SlowOnDamage(float maxSpeed, float speedPenaltyPerShot, float speedPenaltyDuration, float speedPenaltyHealDelay, float minimumSpeed)
    {
        _maxSpeed = maxSpeed;
        _speedPenaltyPerShot = speedPenaltyPerShot;
        _speedPenaltyDuration = speedPenaltyDuration;
        _speedPenaltyHealDelay = speedPenaltyHealDelay;
        _minimumSpeed = minimumSpeed;
    }
    public void OnAdded(NetworkPlayer networkPlayer)
    {
        _networkPlayer = networkPlayer;
    }
    
    public void OnRemoved(NetworkPlayer networkPlayer)
    {
        _networkPlayer = null;
    }
    
    public void Update(float delta)
    {
        if (_networkPlayer is not { HasRig: true })
            return;
        
        Executor.RunIfMe(_networkPlayer.PlayerID, () =>
        {
            if (_speedHealDelay > 0f)
            {
                _speedHealDelay -= delta;
                return;
            }

            // Heal speed penalty over time
            if (_speedModifier >= 1f)
                return;
        
            _speedModifier += delta / _speedPenaltyDuration;
            LocalSpeed.SpeedModifier = _speedModifier;
        });
    }
    
    public void OnDamageTaken(Attack attack, PlayerID? source)
    {
        if (_networkPlayer is not { HasRig: true })
            return;
        
        Executor.RunIfMe(_networkPlayer.PlayerID, () =>
        {
            if (source == null)
                return;
        
            if (!source.IsTeam<HiderTeam>())
                return;

            _speedModifier = MathF.Max(_speedModifier - _speedPenaltyDuration, _minimumSpeed);
            _speedHealDelay = _speedPenaltyHealDelay;
            LocalSpeed.SpeedModifier = _speedModifier;

            // If we can drop players on max damage
            if (!Gamemode.TheHunt.Config.DropPlayer)
                return;
        
            if (_speedModifier > _minimumSpeed) 
                return;
        
            PlayerUtils.DropPlayers();
        });
    }
    
    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _maxSpeed);
        serializer.SerializeValue(ref _speedPenaltyPerShot);
        serializer.SerializeValue(ref _speedPenaltyDuration);
        serializer.SerializeValue(ref _speedPenaltyHealDelay);
        serializer.SerializeValue(ref _minimumSpeed);
    }
}