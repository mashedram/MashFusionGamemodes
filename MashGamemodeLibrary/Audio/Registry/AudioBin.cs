using Il2CppSLZ.Marrow.Warehouse;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Audio.Registry;

public class AudioBin
{
    public string Tag { get; init; }
    public ulong TagHash { get; init; }
    private readonly List<string> _barcodes = new();
    private readonly List<string> _fallbackBarcodes = new();
    
    public AudioBin(string tag, List<string> fallbackBarcodes)
    {
        if (fallbackBarcodes.Count == 0)
            InternalLogger.Debug($"No fallback audio found for audio tag: {tag}");
        
        Tag = tag;
        TagHash = tag.GetStableHash();
    }

    public void Add(string barcode)
    {
        _barcodes.Add(barcode);
    }

    public IReadOnlyList<string> GetAll()
    {
        return _barcodes.Count > 0 ? _barcodes : _fallbackBarcodes;
    }
}