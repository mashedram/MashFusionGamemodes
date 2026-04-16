using System.Collections.Immutable;
using Il2CppSLZ.Marrow;

namespace MashGamemodeLibrary.Entities.Interaction.Balancing;

public enum WeaponType
{
    Pistol,
    Smg,
    Rifle,
    Shotgun
}

public class GunInfo
{
    public float RoundsPerSecond { get; init; }
    public Gun.FireMode FireMode { get; init; }
    public WeaponType WeaponType { get; init; }
    public float Recoil { get; init; }

    public float BulletCount { get; init; }
    public float SpreadAngle { get; init; }

    public float BulletVelocity { get; init; }
    public float BulletMass { get; init; }

    public GunInfo(Gun gun)
    {
        WeaponType = GetWeaponType(gun);
        FireMode = gun.fireMode;
        RoundsPerSecond = gun.roundsPerSecond;
        Recoil =
            gun.pullAnimationSpeed * gun.pullAnimationPerc -
            gun.returnAnimationSpeed * gun.returnAnimationPerc;

        var projectileData = gun.defaultCartridge.projectile;
        if (projectileData == null)
            return;

        BulletCount = projectileData.count;
        SpreadAngle = projectileData.angle;
        BulletVelocity = projectileData.startVelocity;
        BulletMass = projectileData.mass;
    }

    // Helpers
    private static readonly ImmutableDictionary<string, WeaponType> WeaponTypeLookup = ImmutableDictionary.CreateRange(new[]
    {
        KeyValuePair.Create("Pistol", WeaponType.Pistol),
        KeyValuePair.Create("SMG", WeaponType.Smg),
        KeyValuePair.Create("Shotgun", WeaponType.Shotgun),
        KeyValuePair.Create("Rifle", WeaponType.Rifle)
    });

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
}