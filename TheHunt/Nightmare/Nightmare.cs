using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Combat;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow.Extenders;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Association.Impl;
using MashGamemodeLibrary.Entities.Behaviour;
using MashGamemodeLibrary.Entities.Behaviour.Cache;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Queries;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Actions;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;
using MashGamemodeLibrary.Registry.Typed;
using MashGamemodeLibrary.Util;
using TheHunt.Nightmare.Ability;
using TheHunt.Phase;
using TheHunt.Player.Speed;
using TheHunt.Teams;

namespace TheHunt.Nightmare;

internal readonly record struct ActiveNightmare(ulong Key, INightmareDescriptor NightmareDescriptor);

// For reference based mutability
internal class AbilityCooldownTimer
{
    public float Timer { get; set; }
    
    public AbilityCooldownTimer(float timer = 0f)
    {
        Timer = timer;
    }
}

public class Nightmare : IPlayerAttached, IRemoved, IUpdate, IPlayerInputCallback, INetSerializable
{
    private static readonly FactoryTypedRegistry<INightmareDescriptor> NightmareRegistry = new FactoryTypedRegistry<INightmareDescriptor>();
    public static INightmareDescriptor? LocalNightmare { get; private set; }
    private ulong _networkedNightmare;
    
    // Association
    public static readonly CachedQuery<Nightmare> Nightmares = CachedQueryManager.Create<Nightmare>();
    private static readonly IAssociatedBehaviourCache<NetworkEntityAssociation, IAbility> AbilityCache = 
        BehaviourManager.CreateCache<NetworkEntityAssociation, IAbility>();
    
    // Player component
    private NetworkPlayer? _player;
    
    // The nightmare that is currently applied with the component
    private ActiveNightmare? _activeNightmare = null;
    public INightmareDescriptor? Descriptor => _activeNightmare?.NightmareDescriptor;
    
    // Loaded abilities
    private readonly Dictionary<IActiveAbility, AbilityCooldownTimer> _abilityCooldowns = new Dictionary<IActiveAbility, AbilityCooldownTimer>();
    
    // Default Constructor for Serialization
    public Nightmare() {}

    public Nightmare(INightmareDescriptor nightmareDescriptor)
    {
        _networkedNightmare = NightmareRegistry.GetID(nightmareDescriptor);
        ApplyNightmare(nightmareDescriptor);
    }

    public static Nightmare AsRandomNightmare()
    {
        if (NightmareRegistry.Count == 0)
            throw new InvalidOperationException("No nightmares registered in the registry.");
        
        var nightmare = NightmareRegistry.GetAllTypes().GetRandom();
        return new Nightmare(NightmareRegistry.Get(nightmare)!);
    }

    public static void RegisterAll<T>()
    {
        NightmareRegistry.RegisterAll<T>();
    }
    
    public void OnReady(NetworkPlayer networkPlayer)
    {
        _player = networkPlayer;
        CheckNightmare();
    }
    
    public void OnRemoved()
    {
        if (_player == null || _player?.PlayerID?.IsValid != true)
            return;
        
        if (_player.PlayerID.IsMe)
        {
            LocalNightmare = null;
            
            if (Gamemode.TheHunt.Config.SetNightmareAvatars)
                LocalAvatar.AvatarOverride = null;
            
            AvatarStatManager.ResetStats();
            LocalSpeed.SpeedModifier = 1f;

            NightVisionHelper.Enabled = false;
        }
        
        PurgeAbilities();
    }
    
    public void Update(float delta)
    {
        if (_player == null || _player?.PlayerID?.IsValid != true)
            return;
        
        if (!_activeNightmare.HasValue)
            return;
        
        if (!_player.PlayerID.IsMe)
            return;

        foreach (var abilityCooldownsValue in _abilityCooldowns.Values)
        {
            abilityCooldownsValue.Timer -= delta;
        }
    }
    
    public void OnInput(PlayerInputType type, bool state, Handedness handedness)
    {
        if (_player == null || _player?.PlayerID?.IsValid != true)
            return;
        
        if (!_player.PlayerID.IsMe)
            return;
        
        if (GamePhaseManager.ActivePhase is HidePhase)
            return;

        if (type != PlayerInputType.Ability) 
            return;
        
        foreach (var activeAbility in GetAbilities().OfType<IActiveAbility>())
        {
            if (activeAbility.Handedness != handedness && activeAbility.Handedness == Handedness.BOTH)
                continue;
            
            var cooldown = _abilityCooldowns.GetValueOrCreate(activeAbility, () => new AbilityCooldownTimer());
            if (cooldown.Timer > 0f)
            {
                Notifier.CancelAll();
                Notifier.Send(new Notification()
                {
                    Title = "Ability on Cooldown",
                    Message = $"Ability will be ready in {Math.Ceiling(cooldown.Timer)} seconds.",
                    PopupLength = 1f
                });
                return;
            }
                
            activeAbility.UseAbility(this, _player);
            cooldown.Timer = activeAbility.Cooldown;
        }
    }
    
    private IEnumerable<IAbility> GetAbilities()
    {
        if (_player == null || _player?.PlayerID?.IsValid != true)
            return Enumerable.Empty<IAbility>();
        
        return AbilityCache.GetAll(_player.PlayerID.SmallID);
    }

    private void PurgeAbilities()
    {
        if (_player == null)
            return;

        Executor.RunIfHost(() =>
        {
            AbilityCache
                .ForEach(_player.PlayerID.SmallID, (holder, ability) =>
                {
                    EcsManager.Remove(holder.Index); 
                });
        });
    }

    private void AssignAbilities(INightmareDescriptor nightmare)
    {
        Executor.RunIfHost(() =>
        {
            if (_player == null)
                return;

            PurgeAbilities();
            foreach (var component in nightmare.AbilityFactories)
            {
                _player.AddComponent(component());
            }
        });
    }

    // NOT NETWORKED
    private void ApplyNightmare(INightmareDescriptor nightmareDescriptor)
    {
        var wantedKey = NightmareRegistry.GetID(nightmareDescriptor);
        // Check if the nightmare is already applied
        if (_activeNightmare.HasValue && _activeNightmare.Value.Key == wantedKey)
            return;
        
        // Check the player and its validity
        if (_player == null || _player?.PlayerID?.IsValid != true)
            return;
        
        _activeNightmare = new ActiveNightmare(wantedKey, nightmareDescriptor);
        
        // Apply values
        var nightmare = _activeNightmare.Value.NightmareDescriptor;
        if (_player.PlayerID.IsMe)
        {
            LocalNightmare = nightmare;
            
            if (Gamemode.TheHunt.Config.SetNightmareAvatars)
            {
                LocalAvatar.AvatarOverride = nightmare.AvatarBarcode;
            }
            
            AvatarStatManager.SetStats(nightmare.AvatarStats);
            
            NightVisionHelper.Enabled = Gamemode.TheHunt.Config.NightVision;
            NightVisionHelper.Brightness = Gamemode.TheHunt.Config.NightVisionBrightness;
        }
        
        // Remove old abilities
        AssignAbilities(nightmare);
        
        Executor.RunIfMe(_player, () =>
        {
            var descriptionMessage = GetAbilities()
                .OfType<IActiveAbility>()
                .Aggregate("You can:\n", (current, ability) => current + $"{ability.Handedness.ToString()}: {ability.Description}\n");

            Notifier.Send(new Notification
            {
                Title = $"You are the: {nightmare.Name}",
                Message = descriptionMessage,
                PopupLength = 10,
                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.INFORMATION
            });
        });
    }

    private void CheckNightmare()
    {
        if (_player == null)
            return;
        
        if (!NightmareRegistry.TryGet(_networkedNightmare, out var nightmareDescriptor))
            return;
        
        ApplyNightmare(nightmareDescriptor);
    }
    
    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _networkedNightmare);
        
        if (!serializer.IsReader)
            CheckNightmare();
    }
}