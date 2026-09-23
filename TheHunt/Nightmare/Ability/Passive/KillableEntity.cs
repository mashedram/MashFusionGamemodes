using Il2CppSLZ.Marrow.Combat;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player.Actions;
using MashGamemodeLibrary.Player.Data;
using MashGamemodeLibrary.Player.Data.Rules.Rules;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Player.Speed;

namespace TheHunt.Nightmare.Ability.Passive;

public class KillableEntity : IAbility, IPlayerInputCallback, IPlayerAttached, IRemoved, IPlayerTakeDamageCallback, IUpdate
{
    private readonly float _healCooldown;
    private readonly float _respawnCooldown;
    private readonly int _maxHitPoints;
    private readonly float _respawnDistance;

    public KillableEntity()
    {
        _healCooldown = 5f;
        _respawnCooldown = 20f;
        _maxHitPoints = 3;
        _respawnDistance = 20f;
    }
    
    public KillableEntity(float healCooldown, int maxHitPoints, float respawnCooldown, float respawnDistance = 20f)
    {
        _healCooldown = healCooldown;
        _maxHitPoints = maxHitPoints;
        _respawnCooldown = respawnCooldown;
        _respawnDistance = respawnDistance;
    }
    
    private NetworkPlayer? _player;

    private bool _isDead = false;
    private float _timeUntilHeal = 0f;
    private int _hitPoints = 0;

    public void OnReady(NetworkPlayer networkPlayer)
    {
        _player = networkPlayer;
    }

    private void SetHiding(bool state)
    {
        if (_player is not { HasRig: true})
            return;
        
        Executor.RunIfHost(() =>
        {
            var data = PlayerDataManager.GetPlayerData(_player);
            if (data == null)
                return;
            
            data.GetRuleInstance<PlayerSpectatingRule>().Modify(p => p.IsSpectating = state);
        });
    }
    
    public void OnRemoved()
    {
        if (_player is not { HasRig: true})
            return;

        Executor.RunIfHost(() =>
        {
            SetHiding(false);
        });
    }
    
    public void OnDamageTaken(Attack attack, PlayerID? source)
    {
        if (_player is not { HasRig: true})
            return;

        Executor.RunIfHost(() =>
        {
            if (_isDead)
                return;

            _hitPoints -= 1;
            _timeUntilHeal = _healCooldown;

            if (_hitPoints > 0) 
                return;
            
            _isDead = true;
            _timeUntilHeal = _respawnCooldown;
            SetHiding(true);
        });
    }

    private bool CanRespawn()
    {
        return NetworkPlayer.Players
            .Where(player => player.HasRig && !player.PlayerID.IsTeamMember() && !player.IsSpectating())
            .Any(p => p.DistanceSqr > _respawnDistance * _respawnDistance);
    }
    
    public void Update(float delta)
    {
        if (_player is not { HasRig: true})
            return;

        Executor.RunIfMe(_player.PlayerID, () =>
        {
            if (_isDead)
            {
                if (_timeUntilHeal <= 0f)
                    return;
                
                _timeUntilHeal -= delta;
            }
            
            if (_hitPoints >= _maxHitPoints)
                return;

            _timeUntilHeal -= delta;
            if (_timeUntilHeal > 0f)
                return; 
            
            _hitPoints = _maxHitPoints;
        });
    }
    
    public void OnInput(PlayerInputType type, bool state, Handedness handedness)
    {
        if (_player is not { HasRig: true})
            return;
        
        if (type != PlayerInputType.Ability)
            return;
        
        if (!state)
            return;
        
        Executor.RunIfHost(() =>
        {
            if (!_isDead)
                return;

            if (_timeUntilHeal > 0f)
                return;

            if (!CanRespawn())
                return;

            _isDead = false;
            _hitPoints = _maxHitPoints;
            SetHiding(false);
        });
    }
}