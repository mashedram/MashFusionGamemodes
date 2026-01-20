using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Marrow;
using MashGamemodeLibrary.Audio.Registry;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Audio.Music;

public enum MusicPackTags
{
    Combat,
    Ambient
}

public static class MusicPackManager
{
    private const string MusicPackPrefix = "MusicPack.";

    private static readonly Dictionary<MusicPackTags, AudioBin> AudioBins = new Dictionary<MusicPackTags, AudioBin>()
    {
        {
            MusicPackTags.Combat,
            AudioRegistry.CreateBin("MusicPack.Combat", FusionMonoDiscReferences.CombatSongReferences.Select(s => s._barcode._id).ToArray())
        },
        {
            MusicPackTags.Ambient,
            AudioRegistry.CreateBin("MusicPack.Ambient", FusionMonoDiscReferences.FistfightFusionReference._barcode._id)
        }
    };

    private static readonly Dictionary<MusicPackTags, List<string>> TagToDiscs = new();
    
    public static IReadOnlyList<string> GetTracks(MusicPackTags tag)
    {
        return AudioBins.GetValueOrDefault(tag)?.GetAll() ?? Array.Empty<string>();
    }
}