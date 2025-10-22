using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Menu.Data;
using MashGamemodeLibrary.Config;
using MashGamemodeLibrary.Config.Constraints;
using MashGamemodeLibrary.Config.Menu;
using MashGamemodeLibrary.Loadout;
using MashGamemodeLibrary.Util;

namespace BoneStrike.Config;

internal class SecondsToMinutesDisplayTransformer : IConfigDisplayTransformer
{
    public object ToDisplay(object value)
    {
        return (float)value / 60f;
    }

    public object FromDisplay(object display)
    {
        return (float)display * 60;
    }
}


public class BoneStrikeConfig : AutoSerialized<BoneStrikeConfig>, IConfig, IConfigMenuProvider
{
    [ConfigMenuEntry("Plant Phase Duration")]
    [ConfigStepSize(15f)]
    [ConfigDisplayTransformer(typeof(float), typeof(SecondsToMinutesDisplayTransformer))]
    [ConfigRangeConstraint(15f, 600f)]
    public float PlantDuration = 60f;
    
    [ConfigMenuEntry("Defuse Phase Duration")]
    [ConfigStepSize(15f)]
    [ConfigDisplayTransformer(typeof(float), typeof(SecondsToMinutesDisplayTransformer))]
    [ConfigRangeConstraint(15f, 600f)]
    public float DefuseDuration = 60f;

    [ConfigMenuEntry("Max Respawns")]
    [ConfigRangeConstraint(0, 3)]
    public int MaxRespawns = 0;

    public string PalletBarcode;
    
    public void AddExtraFields(GroupElementData root)
    {
        root.AddElement(new SpawnableElementData
        {
            Title = "Spawnable",
            OnSetSpawnable = barcode =>
            {
                if (!AssetWarehouse.Instance.TryGetCrate(new Barcode(barcode), out var crate))
                    return;

                PalletBarcode = crate._pallet._barcode._id;
            }
        });
    }
}