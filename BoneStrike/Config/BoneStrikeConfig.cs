using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Menu.Data;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Config;
using MashGamemodeLibrary.Config.Constraints;
using MashGamemodeLibrary.Config.Menu;
using MashGamemodeLibrary.Config.Menu.Attributes;
using MashGamemodeLibrary.Loadout;
using MashGamemodeLibrary.Util;

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



public class BoneStrikeConfig : IConfig
{
    [ConfigMenuEntry("Plant Phase Duration")]
    [ConfigElementProvider(typeof(SecondsToMinutesElementProvider))]
    public float PlantDuration = 60f;
    
    [ConfigMenuEntry("Defuse Phase Duration")]
    [ConfigElementProvider(typeof(SecondsToMinutesElementProvider))]
    public float DefuseDuration = 60f;

    [ConfigMenuEntry("Max Respawns")]
    [ConfigRangeConstraint(0, 3)]
    public int MaxRespawns = 0;

    [ConfigMenuEntry("Defuse timer")] 
    [ConfigRangeConstraint(2f, 20f)]
    [SerializableField]
    public float DefuseTime = 7f;
    
    [ConfigMenuEntry("Health Multiplier")]
    [ConfigRangeConstraint(0.25f, 4f)]
    [ConfigStepSize(0.25f)]
    [SerializableField]
    public float HealthMultiplier = 1f;

    [ConfigMenuEntry("Balance Weapon Damage")]
    public bool BalanceDamage = true;
    
    [ConfigMenuEntry("Damage Multiplier")]
    [ConfigRangeConstraint(0.25f, 4f)]
    [ConfigStepSize(0.25f)]
    [SerializableField]
    public float DamageMultiplier = 1f;
    
    [ConfigMenuEntry("Dev Tools Disabled")]
    [SerializableField]
    public bool DevToolsDisabled = true;

    [ConfigMenuEntry("Set Spawnable")]
    [ConfigElementProvider(typeof(CrateBarcodeElement))]
    public string PalletBarcode = "";

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlantDuration);
        serializer.SerializeValue(ref DefuseDuration);
        serializer.SerializeValue(ref MaxRespawns);
        serializer.SerializeValue(ref DefuseDuration);
        serializer.SerializeValue(ref HealthMultiplier);
        serializer.SerializeValue(ref BalanceDamage);
        serializer.SerializeValue(ref DamageMultiplier);
        serializer.SerializeValue(ref DevToolsDisabled);
        serializer.SerializeValue(ref PalletBarcode);
    }
}