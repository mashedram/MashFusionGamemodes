using Il2CppSLZ.Marrow.Interaction;
using Il2CppTMPro;
using LabFusion.Entities;
using LabFusion.RPC;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.Interaction.Attacking;
using MashGamemodeLibrary.Entities.Interaction.Grabbing;
using MashGamemodeLibrary.Entities.Queries;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Team;
using MelonLoader;
using TheHunt.Phase;
using TheHunt.Teams;
using UnityEngine;

namespace TheHunt.Components;

public class ObjectiveItemComponent : IEntityAttached, IAttackReceiver, IGrabPredicate
{
    public static readonly CachedQuery<ObjectiveItemComponent> Query = CachedQueryManager.Create<ObjectiveItemComponent>();
    
    private float _maxHealth = 15f;
    private float _health;
    private bool _isDestroyed;

    private NetworkEntity? _networkEntity;
    private MarrowEntity? _marrowEntity;
    
    private TextMeshPro? _clockText;
    
    public Vector3 Position => _marrowEntity?.transform.position ?? Vector3.zero;
    
    private void UpdateText()
    {
        if (_clockText == null)
            return;
        
        var percent = _health / _maxHealth;
        _clockText.text = $"{percent * 100f:0}%";
    }
    
    public void OnReady(NetworkEntity networkEntity, MarrowEntity marrowEntity)
    {
        _networkEntity = networkEntity;
        _marrowEntity = marrowEntity;
        
        _maxHealth = Gamemode.TheHunt.Config.ObjectiveHealth;
        _health = _maxHealth;
        
        _clockText = marrowEntity.GetComponentInChildren<TextMeshPro>();
        UpdateText();
    }
    
    public void ReceiveAttack(float damage, NetworkPlayer attacker)
    {
        // Don't let nightmares attack the objective item
        if (attacker.PlayerID.IsTeam<NightmareTeam>())
            return;
        
        _health -= damage;
        UpdateText();

        if (_health > 0f || _isDestroyed) 
            return;
        
        _isDestroyed = true;
        GameAssetSpawner.Despawn(_networkEntity);

    }
    
    public bool CanGrab(GrabRequest grabRequest)
    {
        var isNightmare = grabRequest.NetworkPlayer.PlayerID.IsTeam<NightmareTeam>();
        if (GamePhaseManager.ActivePhase is PlantPhase)
            return isNightmare;
        
        return !isNightmare;
    }
}