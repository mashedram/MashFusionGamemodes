using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Marrow;
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

    private static readonly Dictionary<MusicPackTags, string> DefaultMusicPacks = new()
    {
        {
            MusicPackTags.Combat, "FusionCombatMusic"
        }
    };

    private static readonly Dictionary<MusicPackTags, List<string>> TagToDiscs = new();

    private static MusicPackTags? GetPackType(Pallet pallet)
    {
        var description = pallet._description;
        if (!description.StartsWith(MusicPackPrefix, StringComparison.Ordinal))
            return null;

        var tagString = string.Concat(description.Skip(MusicPackPrefix.Length).TakeWhile(char.IsLetterOrDigit));
        return Enum.TryParse(tagString, true, out MusicPackTags tag) ? tag : null;
    }

    private static void RegisterPack(MusicPackTags tag, IMusicPack pack)
    {
        var packList = TagToDiscs.GetOrCreate(tag, () => new List<string>());
        packList.AddRange(pack.LoadTracks());
    }

    public static void LoadPacks()
    {
        TagToDiscs.Clear();
        // Inject static music

        RegisterPack(MusicPackTags.Combat, new MusicPack(FusionMonoDiscReferences.CombatSongReferences));

        // Load custom packages

        foreach (var pallet in AssetWarehouse.Instance.GetPallets())
        {
            var tag = GetPackType(pallet);
            if (tag == null)
                continue;

            RegisterPack((MusicPackTags)tag, new MusicPalletPack(pallet));
        }
    }

    public static List<string> GetTracks(MusicPackTags tag)
    {
        return TagToDiscs.GetOrCreate(tag, () => new List<string>());
    }
}