using Il2CppInterop.Runtime;
using Il2CppSLZ.Marrow.Warehouse;
using Type = Il2CppSystem.Type;

namespace MashGamemodeLibrary.Audio.Music;

public record MusicPalletPack(Pallet Pallet) : IMusicPack
{
    private static readonly Type MonoDiscType = Il2CppType.Of<MonoDisc>();

    public List<string> LoadTracks()
    {
        return Pallet.DataCards.ToArray()
            .Where(p => p.GetIl2CppType() == MonoDiscType)
            .Select(p => p._barcode._id)
            .ToList();
    }
}