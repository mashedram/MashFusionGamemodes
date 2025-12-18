using Il2CppInterop.Runtime;
using Il2CppSLZ.Marrow.Warehouse;

namespace MashGamemodeLibrary.Audio.Music;

public record MusicPalletPack(Pallet Pallet) : IMusicPack
{
    private static readonly Il2CppSystem.Type MonoDiscType = Il2CppType.Of<MonoDisc>();

    public List<string> LoadTracks()
    {
        return Pallet.DataCards.ToArray()
            .Where(p => p.GetIl2CppType() == MonoDiscType)
            .Select(p => p._barcode._id)
            .ToList();
    }
}