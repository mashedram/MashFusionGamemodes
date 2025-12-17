using System.Collections.Immutable;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Extensions;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Registry.Keyed;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace MashGamemodeLibrary.Entities.Interaction;

enum WeaponType
{
    Pistol,
    Smg,
    Rifle,
    Shotgun
}

class DamageRemapper
{
    private readonly float _lowValue;
    private readonly float _highValue;

    private readonly float _lowTarget;
    private readonly float _highTarget;

    private readonly float _ttkMultiplier;
    
    public DamageRemapper(float ttkMultiplier, float lowValue, float highValue, float target, float deviation = 0.5f)
    {
        _ttkMultiplier = ttkMultiplier;
        _lowValue = lowValue;
        _highValue = highValue;

        var halfDeviation = deviation / 2f;
        _lowTarget = Math.Max(target - halfDeviation, 0f);
        _highTarget = Math.Max(target + halfDeviation, 0f);
    }

    public float GetDamage(float damage, float roundsPerSecond, Gun.FireMode fireMode, float mult = 1f)
    {
        var clamped = MathUtil.Clamp(damage, _lowValue, _highValue);

        const float baseRpm = 12f;
        var normalizedRoundsPerSecond = roundsPerSecond * fireMode switch
        {
            Gun.FireMode.SEMIAUTOMATIC => 0.5f,
            Gun.FireMode.MANUAL => 0.25f,
            _ => 1f
        } / baseRpm;

        var flat = MathUtil.Lerp(_lowTarget, _highTarget, clamped);
        var normalized = flat / (normalizedRoundsPerSecond * _ttkMultiplier);
        
        return normalized * _ttkMultiplier * mult;
    }
}

public static class PlayerGunManager
{
    public delegate void OnGunFiredHandler(NetworkPlayer shooter, Gun gun);

    public static event OnGunFiredHandler? OnGunFired;

    private static readonly ImmutableDictionary<string, WeaponType> WeaponTypeLookup = ImmutableDictionary.CreateRange(new[]
    {
        KeyValuePair.Create("Pistol", WeaponType.Pistol),
        KeyValuePair.Create("SMG", WeaponType.Smg),
        KeyValuePair.Create("Shotgun", WeaponType.Shotgun),
        KeyValuePair.Create("Rifle", WeaponType.Rifle)
    });
    private static readonly KeyedRegistry<WeaponType, DamageRemapper> DamageRemappers = new();
    private static readonly Dictionary<Gun, float> DefaultGunDamage = new(new UnityComparer());
    private static readonly Dictionary<Gun, float> CachedGunDamage = new(new UnityComparer());

    private static bool _normalizePlayerDamage;
    public static bool NormalizePlayerDamage
    {
        get => _normalizePlayerDamage;
        set
        {
            _normalizePlayerDamage = value;
            if (_normalizePlayerDamage)
                return;
            
            foreach (var gun in CachedGunDamage.Keys.OfType<Gun>())
            {
                if (!DefaultGunDamage.TryGetValue(gun, out var damage))
                    continue;

                gun.defaultCartridge.projectile.damageMultiplier = damage;
            }
            CachedGunDamage.Clear();
        }
    }
    
    private static float _damageMultiplier = 1f;
    public static float DamageMultiplier
    {
        get => _damageMultiplier;
        set
        {
            _damageMultiplier = value;
            CachedGunDamage.Clear();
        }
    }

    static PlayerGunManager()
    {
        DamageRemappers.Register(WeaponType.Pistol, new DamageRemapper(
            0.9f,
            0.6f,
            1f,
            1f
        ));
        DamageRemappers.Register(WeaponType.Smg, new DamageRemapper(
            1.20f,
            0.4f,
            0.9f,
            0.8f,
            0.8f
        ));
        DamageRemappers.Register(WeaponType.Shotgun, new DamageRemapper(
            1f,
            1f,
            2f,
            1.1f,
            0.3f
        ));
        DamageRemappers.Register(WeaponType.Rifle, new DamageRemapper(
            1.1f,
            0.6f,
            2f,
            1.1f,
            0.7f
        ));
    }

    private static WeaponType GetWeaponType(Gun gun)
    {
        var crate = gun._poolee?.SpawnableCrate;
        if (crate == null)
            return WeaponType.Rifle;
        
        foreach (var tag in crate._tags)
        {
            if (tag == null) continue;

            if (WeaponTypeLookup.TryGetValue(tag, out var type))
                return type;
        }

        return WeaponType.Rifle;
    }
    
    private static float GetGunDamageMultiplier(Gun gun)
    {
        if (CachedGunDamage.TryGetValue(gun, out var value))
            return value;
        
        
        
        var defaultDamage = DefaultGunDamage.GetOrCreate(gun, () => gun.defaultCartridge.projectile.damageMultiplier);
        var type = GetWeaponType(gun);
        
        // We register all types at the start
        var damage = DamageRemappers.Get(type)!.GetDamage(defaultDamage, gun.roundsPerSecond, gun.fireMode, _damageMultiplier);
        CachedGunDamage[gun] = damage;

        return damage;
    }

    private static void NormalizeGunDamage(Gun gun)
    {
        gun.defaultCartridge.projectile.damageMultiplier = GetGunDamageMultiplier(gun);
    }

    public static void OnGunGrabbed(Gun gun)
    {
        if (_normalizePlayerDamage)
            NormalizeGunDamage(gun);
    }

    public static void InvokeGunFired(Gun instance)
    {
        var triggerGrip = instance.triggerGrip;
        if (triggerGrip == null)
            return;

        var attachedHands = triggerGrip.attachedHands;
        if (attachedHands.Count == 0) return;

        var holder = attachedHands._items[0];

        if (!holder)
            return;

        var rigManager = holder.manager;
        if (!rigManager)
            return;

        if (!NetworkPlayerManager.TryGetPlayer(rigManager, out var player))
            return;

        OnGunFired?.Invoke(player, instance);
    }
    
    public static void Reset()
    {
        DefaultGunDamage.Clear();
        CachedGunDamage.Clear();
        _damageMultiplier = 0f;
        _normalizePlayerDamage = false;
    }
}