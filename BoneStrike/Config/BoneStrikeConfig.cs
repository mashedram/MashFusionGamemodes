using System.Text.Json.Serialization;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Data;
using LabFusion.Menu.Data;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Config;
using MashGamemodeLibrary.Config.Menu;
using MashGamemodeLibrary.Config.Menu.Attributes;
using MashGamemodeLibrary.Loadout;
using UnityEngine;

namespace BoneStrike.Config;

internal class SecondsToMinutesElementProvider : IConfigElementProvider
{
    public ElementData GetElementData(ConfigEntryData entry, Action<ConfigEntryData, object> setter)
    {
        return new FloatElementData
        {
            Title = entry.Name,
            Increment = 0.25f,
            MaxValue = 10f,
            MinValue = 0.25f,
            Value = Convert.ToSingle(entry.Value) / 60f,
            OnValueChanged = f => setter(entry, Convert.ToSingle(f) * 60f)
        };
    }
}

internal class PositionElementProvider : IConfigElementProvider
{
    public ElementData GetElementData(ConfigEntryData entry, Action<ConfigEntryData, object> setter)
    {
        return new FunctionElementData
        {
            Title = entry.Name,
            OnPressed = () =>
            {
                var np = LocalPlayer.GetNetworkPlayer();
                if (np is not { HasRig: true })
                    return;

                var position = np.RigRefs.RigManager.transform.position;
                setter.Invoke(entry, position);
            }
        };
    }
}

internal class CrateBarcodeElement : IConfigElementProvider
{
    public ElementData GetElementData(ConfigEntryData entry, Action<ConfigEntryData, object> setter)
    {
        return new SpawnableElementData
        {
            Title = entry.Name,
            OnSetSpawnable = barcode =>
            {
                if (!AssetWarehouse.Instance.TryGetCrate(new Barcode(barcode), out var crate))
                    return;

                setter.Invoke(entry, crate._pallet._barcode._id);
            }
        };
    }
}

internal class CrateBarcodeListElement : IConfigElementProvider
{
    public ElementData GetElementData(ConfigEntryData entry, Action<ConfigEntryData, object> setter)
    {
        var group = new GroupElementData(entry.Name);

        var add = new SpawnableElementData
        {
            Title = "Add Held Item Pallet",
            OnSetSpawnable = barcode =>
            {
                if (!AssetWarehouse.Instance.TryGetCrate(new Barcode(barcode), out var crate))
                    return;

                var list = (List<string>)entry.Value;

                var id = crate._pallet._barcode._id;
                if (list.Contains(id))
                    return;

                list.Add(id);
                setter.Invoke(entry, list);
            }
        };

        var clear = new FunctionElementData
        {
            Title = "Clear",
            OnPressed = () =>
            {
                setter.Invoke(entry, new List<string>());
            }
        };

        group.AddElement(add);
        group.AddElement(clear);

        return group;
    }
}

internal class BarcodeListElement : IConfigElementProvider
{
    public ElementData GetElementData(ConfigEntryData entry, Action<ConfigEntryData, object> setter)
    {
        var group = new GroupElementData(entry.Name);

        var add = new SpawnableElementData
        {
            Title = "Add Held Item",
            OnSetSpawnable = barcode =>
            {
                var list = (List<string>)entry.Value;

                var id = barcode._id;
                if (list.Contains(id))
                    return;

                list.Add(id);
                setter.Invoke(entry, list);
            }
        };

        var clear = new FunctionElementData
        {
            Title = "Clear",
            OnPressed = () =>
            {
                setter.Invoke(entry, new List<string>());
            }
        };

        group.AddElement(add);
        group.AddElement(clear);

        return group;
    }
}

public class BoneStrikeConfig : IConfig
{
    [ConfigMenuEntry("Plant Phase Duration")] [ConfigElementProvider(typeof(SecondsToMinutesElementProvider))]
    [JsonInclude]
    public float PlantDuration = 60f;
    
    [ConfigMenuEntry("Defuse Phase Duration")] [ConfigElementProvider(typeof(SecondsToMinutesElementProvider))]
    [JsonInclude]
    public float DefuseDuration = 60f;

    [ConfigMenuEntry("Defuse timer")] [ConfigRangeConstraint(2f, 20f)]
    [JsonInclude]
    public float DefuseTime = 7f;
    
    [ConfigMenuEntry("Balance Weapon Damage")]
    [JsonInclude]
    public bool BalanceDamage = true;
    [ConfigMenuEntry("Damage Multiplier")] [ConfigRangeConstraint(0.25f, 4f)] [ConfigStepSize(0.25f)]
    [JsonInclude]
    public float DamageMultiplier = 1f;
    [ConfigMenuEntry("Health Multiplier")] [ConfigRangeConstraint(0.25f, 4f)] [ConfigStepSize(0.25f)]
    [JsonInclude]
    public float HealthMultiplier = 1f;
    
    [ConfigMenuEntry("Weapons")] [ConfigElementProvider(typeof(CrateBarcodeListElement))]
    [JsonInclude]
    public List<string> PalletBarcodes = new();

    [ConfigMenuEntry("Uility Items")] [ConfigElementProvider(typeof(BarcodeListElement))]
    [JsonInclude]
    public List<string> UtilityBarcodes = new();
    
    [ConfigMenuEntry("Bomb Explosion Enabled")]
    [JsonInclude]
    public bool BombExplosion = true;
    
    [ConfigMenuEntry("Dev Tools Disabled")]
    [JsonInclude]
    public bool DevToolsDisabled = true;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlantDuration);
        serializer.SerializeValue(ref DefuseDuration);
        serializer.SerializeValue(ref DefuseTime);
        serializer.SerializeValue(ref HealthMultiplier);
        serializer.SerializeValue(ref BalanceDamage);
        serializer.SerializeValue(ref DamageMultiplier);
        serializer.SerializeValue(ref DevToolsDisabled);
    }
    
    public object Clone()
    {
        return new BoneStrikeConfig
        {
            PlantDuration = PlantDuration,
            DefuseDuration = DefuseDuration,
            DefuseTime = DefuseTime,
            BalanceDamage = BalanceDamage,
            DamageMultiplier = DamageMultiplier,
            HealthMultiplier = HealthMultiplier,
            PalletBarcodes = new List<string>(PalletBarcodes),
            UtilityBarcodes = new List<string>(UtilityBarcodes),
            BombExplosion = BombExplosion,
            DevToolsDisabled = DevToolsDisabled
        };
    }
}