using BoneLib;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Extensions;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Loadout;

enum WeaponType
{
    Primary,
    Secondary,
    Tertiary
}

internal class FetchLoadoutPacket : INetSerializable
{
    public string Barcode;
    
    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Barcode);
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
    
    public static void LoadLocal(string barcode)
    {
        var barcode1 = new Barcode(barcode);
        if (!AssetWarehouse.Instance.TryGetPallet(barcode1, out var pallet))
        {
            MelonLogger.Error($"Failed to load pallet with barcode: {barcode}. Pallet not found");
            return;
        }
        
        Items.Clear();
        
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

    private static Barcode? Get(WeaponType type)
    {
        if (!Items.TryGetValue(type, out var list)) return null;

        var crate = IEnumerableExtensions.GetRandom(list);
        return crate.Barcode;
    }

    public static Loadout GetLoadout()
    {
        var primary = Get(WeaponType.Primary);
        var secondary = Get(WeaponType.Secondary);
        var tertiary = Get(WeaponType.Tertiary);

        return new Loadout()
            .SetSlotBarcode(SlotType.RightBack, primary)
            .SetSlotBarcode(SlotType.RightHolster, secondary)
            .SetSlotBarcode(SlotType.Belt, tertiary);
    }
    
    // Networking
    
    public static void Load(string barcode)
    {
        FetchLoadoutEvent.Call(new FetchLoadoutPacket
        {
            Barcode = barcode
        });
    }
    
    public static void AssignAll()
    {
        AssignLoadoutEvent.Call(new DummySerializable());
    }
    
    // Events
    
    private static void OnFetchLoadout(FetchLoadoutPacket packet)
    {
        LoadLocal(packet.Barcode);
    }

    private static void OnAssignLoadout(DummySerializable _)
    {
        var loadout = GetLoadout();
        loadout.Assign();
    }
}