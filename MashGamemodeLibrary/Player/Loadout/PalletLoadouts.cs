using BoneLib;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Extensions;
using MelonLoader;

namespace MashGamemodeLibrary.Loadout;

enum WeaponType
{
    Primary,
    Secondary,
    Tertiary
}

/// <summary>
/// Using a pallet, automatically construct a bunch of loadouts
/// </summary>
public class PalletLoadouts
{
    private static readonly string[] PrimaryTypeTags = new[]
    {
        "SMG",
        "Shotgun",
        "Rifle"
    };
    private static readonly string[] SecondaryTypeTags = {
        "Pistol",
    };

    private static readonly string[] TertiaryTypeTags = {
        "Blade",
        "Blunt"
    };

    private readonly Dictionary<WeaponType, List<Crate>> _items = new();

    private List<Crate> GetCrateList(WeaponType type)
    {
        if (_items.TryGetValue(type, out var list)) return list;

        var newList = new List<Crate>();
        _items[type] = newList;
        return newList;
    }

    private WeaponType? GetCrateType(Crate crate)
    {
        foreach (var tag in crate._tags)
        {
            if (tag == null) continue;

            if (PrimaryTypeTags.Contains(tag))
                return WeaponType.Primary;
            if (SecondaryTypeTags.Contains(tag))
                return WeaponType.Secondary;
            if (TertiaryTypeTags.Contains(tag))
                return WeaponType.Tertiary;
        }
        
        return null;
    }
    
    public PalletLoadouts(string barcode)
    {
        var barcode1 = new Barcode(barcode);
        if (!AssetWarehouse.Instance.TryGetPallet(barcode1, out var pallet))
            return;
        
        foreach (var crate in pallet.Crates)
        {
            if (crate._redacted)
                continue;
            
            var type = GetCrateType(crate);
            if (!type.HasValue)
                continue;
            
            GetCrateList(type.Value).Add(crate);
        }
    }

    private Barcode? Get(WeaponType type)
    {
        if (!_items.TryGetValue(type, out var list)) return null;

        var crate = IEnumerableExtensions.GetRandom(list);
        return crate.Barcode;
    }

    public Loadout GetLoadout()
    {
        var primary = Get(WeaponType.Primary);
        var secondary = Get(WeaponType.Secondary);
        var tertiary = Get(WeaponType.Tertiary);

        return new Loadout()
            .SetSlotBarcode(SlotType.RightBack, primary)
            .SetSlotBarcode(SlotType.RightHolster, secondary)
            .SetSlotBarcode(SlotType.Belt, tertiary);
    }
}