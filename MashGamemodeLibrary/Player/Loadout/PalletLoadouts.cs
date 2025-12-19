using BoneLib;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Extensions;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Loadout;

enum LoadCommandType
{
    Weapon,
    Utility
}

enum WeaponType
{
    Primary,
    Secondary,
    Tertiary,
    Utility
}

internal class FetchLoadoutPacket : INetSerializable
{
    public LoadCommandType LoadCommandType;
    public string[] Barcodes = null!;
    
    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref LoadCommandType);
        serializer.SerializeValue(ref Barcodes);
    }
}

/// <summary>
/// Using a pallet, automatically construct a bunch of loadouts
/// </summary>
public static class PalletLoadoutManager
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

    private static readonly RemoteEvent<FetchLoadoutPacket> FetchLoadoutEvent = new("FetchLoadoutEvent", OnFetchLoadout, CommonNetworkRoutes.HostToAll);
    private static readonly RemoteEvent<DummySerializable> AssignLoadoutEvent = new("AssignLoadoutEvent", OnAssignLoadout, CommonNetworkRoutes.HostToAll);
    
    private static readonly Dictionary<WeaponType, List<Crate>> Items = new();

    private static void ClearWeapons()
    {
        Items.GetValueOrDefault(WeaponType.Primary)?.Clear();
        Items.GetValueOrDefault(WeaponType.Secondary)?.Clear();
        Items.GetValueOrDefault(WeaponType.Tertiary)?.Clear();
        
    }

    private static List<Crate> GetCrateList(WeaponType type)
    {
        if (Items.TryGetValue(type, out var list)) return list;

        var newList = new List<Crate>();
        Items[type] = newList;
        return newList;
    }

    private static WeaponType? GetCrateType(Crate crate)
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
    
    public static void LoadLocal(IEnumerable<string> barcodes)
    {
        ClearWeapons();

        foreach (var barcode in barcodes)
        {
            var barcode1 = new Barcode(barcode);
            if (!AssetWarehouse.Instance.TryGetPallet(barcode1, out var pallet))
            {
                MelonLogger.Error($"Failed to load pallet with barcode: {barcode}: Pallet not found");
                continue;
            }
            
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
    }

    public static void LoadLocalUtility(IEnumerable<string> barcodes)
    {
        var list = GetCrateList(WeaponType.Utility);
        list.Clear();

        foreach (var barcode in barcodes)
        {
            if (!AssetWarehouse.Instance.TryGetCrate(new Barcode(barcode), out var crate)) {
                MelonLogger.Error($"Failed to load crate with barcode: {barcode}: Crate not found");
                continue;
            }
            
            list.Add(crate);
        }
    }

    private static Barcode? Get(WeaponType type)
    {
        if (!Items.TryGetValue(type, out var list)) return null;

        if (list.Count == 0) return null;
        
        var crate = IEnumerableExtensions.GetRandom(list);
        return crate.Barcode;
    }

    public static Player.Loadout.Loadout GetLoadout()
    {
        var primary = Get(WeaponType.Primary);
        var secondary = Get(WeaponType.Secondary);
        var tertiary = Get(WeaponType.Tertiary);
        var utility = Get(WeaponType.Utility);

        return new Player.Loadout.Loadout()
            .SetSlotBarcode(SlotType.RightBack, primary)
            .SetSlotBarcode(SlotType.RightHolster, secondary)
            .SetSlotBarcode(SlotType.LeftBack, utility)
            .SetSlotBarcode(SlotType.Belt, tertiary);
    }
    
    // Networking
    
    public static void Load(string barcode)
    {
        Load(new[] {barcode});
    }
    
    public static void Load(IEnumerable<string> barcodes)
    {
        FetchLoadoutEvent.Call(new FetchLoadoutPacket
        {
            LoadCommandType = LoadCommandType.Weapon,
            Barcodes = barcodes.ToArray()
        });
    }
    
    public static void LoadUtility(IEnumerable<string> barcodes)
    {
        FetchLoadoutEvent.Call(new FetchLoadoutPacket
        {
            LoadCommandType = LoadCommandType.Utility,
            Barcodes = barcodes.ToArray()
        });
    }
    
    public static void AssignAll()
    {
        AssignLoadoutEvent.Call(new DummySerializable());
    }
    
    // Events
    
    private static void OnFetchLoadout(FetchLoadoutPacket packet)
    {
        switch (packet.LoadCommandType)
        {
            case LoadCommandType.Weapon:
                LoadLocal(packet.Barcodes);
                break;
            case LoadCommandType.Utility:
                LoadLocalUtility(packet.Barcodes);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void OnAssignLoadout(DummySerializable _)
    {
        var loadout = GetLoadout();
        loadout.Assign();
    }
}