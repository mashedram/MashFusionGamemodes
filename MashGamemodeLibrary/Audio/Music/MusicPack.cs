using Il2CppSLZ.Marrow.Warehouse;

namespace MashGamemodeLibrary.Audio.Music;

public class MusicPack : IMusicPack
{
    private readonly List<string> _barcodes;

    public MusicPack(List<string> barcodes)
    {
        _barcodes = barcodes;
    }

    public MusicPack(IEnumerable<string> barcodes)
    {
        _barcodes = barcodes.ToList();
    }

    public MusicPack(IEnumerable<MonoDiscReference> barcodes)
    {
        _barcodes = barcodes.Select(disk => disk._barcode._id).ToList();
    }

    public List<string> LoadTracks()
    {
        return _barcodes;
    }
}