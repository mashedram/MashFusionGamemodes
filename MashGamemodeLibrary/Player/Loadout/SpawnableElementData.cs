using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Menu.Data;

namespace MashGamemodeLibrary.Loadout;

public class SpawnableElementData : FunctionElementData
{
    private SpawnableCrateReference? _spawnable;

    public Action<Barcode> OnSetSpawnable = delegate { };

    public new Action OnPressed => OnPressedInternal;

    private static Barcode? GetHeldSpawnableBarcode(Hand hand)
    {
        if (!hand.HasAttachedObject()) return null;

        var poolee = hand.AttachedReceiver.gameObject.GetComponentInParent<Poolee>();
        return !poolee ? null : poolee.SpawnableCrate._barcode;
    }

    private static Barcode? GetHeldSpawnableBarcode()
    {
        var hands = new[]
        {
            BoneLib.Player.LeftHand,
            BoneLib.Player.RightHand
        };

        return (from hand in hands select GetHeldSpawnableBarcode(hand)).FirstOrDefault();
    }

    private void OnPressedInternal()
    {
        var held = GetHeldSpawnableBarcode();
        if (held == null) return;
        _spawnable = new SpawnableCrateReference(held);
        OnSetSpawnable.Invoke(_spawnable._barcode);
    }
}