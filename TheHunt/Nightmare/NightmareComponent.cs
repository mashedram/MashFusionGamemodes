using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.Queries;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Actions;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Registry.Typed;
using MashGamemodeLibrary.Util;
using TheHunt.Nightmare.Ability;
using TheHunt.Phase;

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

public class NightmareComponent : IComponent, IComponentPlayerReady, IComponentRemoved, IComponentUpdate, IPlayerInputCallback, INetSerializable
{
    private static readonly FactoryTypedRegistry<INightmareDescriptor> NightmareRegistry = new FactoryTypedRegistry<INightmareDescriptor>();
    private ulong _networkedNightmare;
    
    // Player component
    private NetworkPlayer? _player;
    
    // The nightmare that is currently applied with the component
    private ActiveNightmare? _activeNightmare = null;
    
    // Loaded abilities
    private readonly List<IAbility> _abilities = new List<IAbility>();
    private readonly Dictionary<IAbility, AbilityCooldownTimer> _abilityCooldowns = new Dictionary<IAbility, AbilityCooldownTimer>();
    
    // Default Constructor for Serialization
    public NightmareComponent() {}

    public NightmareComponent(INightmareDescriptor nightmareDescriptor)
    {
        _networkedNightmare = NightmareRegistry.GetID(nightmareDescriptor);
        ApplyNightmare(nightmareDescriptor);
    }

    public static NightmareComponent AsRandomNightmare()
    {
        if (NightmareRegistry.Count == 0)
            throw new InvalidOperationException("No nightmares registered in the registry.");
        
        var nightmare = NightmareRegistry.GetAllTypes().GetRandom();
        return new NightmareComponent(NightmareRegistry.Get(nightmare)!);
    }

    public static void RegisterAll<T>()
    {
        NightmareRegistry.RegisterAll<T>();
    }
    
    public void OnReady(NetworkPlayer networkPlayer, MarrowEntity marrowEntity)
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
            if (Gamemode.TheHunt.Config.SetNightmareAvatars)
                LocalAvatar.AvatarOverride = null;
            
            AvatarStatManager.ResetStats();

            NightVisionHelper.Enabled = false;
        }
        
        foreach (var ability in _abilities)
        {
            ability.OnRemoved(_player);
        }
        _abilities.Clear();
    }
    
    public void Update(float delta)
    {
        if (_player == null || _player?.PlayerID?.IsValid != true)
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
        
        foreach (var activeAbility in _abilities.OfType<IActiveAbility>())
        {
            if (activeAbility.Handedness != handedness)
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
                
            activeAbility.UseAbility(_player);
            cooldown.Timer = activeAbility.Cooldown;
        }
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
            if (Gamemode.TheHunt.Config.SetNightmareAvatars)
            {
                LocalAvatar.AvatarOverride = nightmare.AvatarBarcode;
            }
            
            AvatarStatManager.SetStats(nightmare.AvatarStats);
            
            NightVisionHelper.Enabled = Gamemode.TheHunt.Config.NightVision;
            NightVisionHelper.Brightness = Gamemode.TheHunt.Config.NightVisionBrightness;
        }
        
        // Remove old abilities
        foreach (var ability in _abilities)
        {
            ability.OnRemoved(_player);
        }
        _abilities.Clear();
        _abilities.AddRange(nightmare.Abilities);
        foreach (var ability in _abilities)
        {
            ability.OnAdded(_player);
        }
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