using Il2CppInterop.Generator.Extensions;
using Il2CppInterop.Runtime;
using Il2CppSLZ.Marrow.Warehouse;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Audio.Registry;

internal class RegisterableAudio
{
    public string Tag { get; init; }
    public ulong TagHash { get; init; }
    public string Barcode { get; init; }

    public RegisterableAudio(string tag, string barcode)
    {
        Tag = tag;
        TagHash = tag.GetStableHash();
        Barcode = barcode;
    }
}

public static class AudioRegistry
{
    private const string TagPrefix = "MashTags.";
    private static readonly Il2CppSystem.Type MonoDiscType = Il2CppType.Of<MonoDisc>();

    private static bool _isRegistered;
    private static readonly List<RegisterableAudio> RegisterableAudios = new();
    private static readonly Dictionary<string, AudioBin> Bins = new();

    private static void PopulateBin(AudioBin bin)
    {
        foreach (var registerableAudio in RegisterableAudios.Where(registerableAudio => registerableAudio.TagHash == bin.TagHash))
        {
            bin.Add(registerableAudio.Barcode);
        }
    }

    private static void BinAudio(RegisterableAudio audio)
    {
        if (Bins.TryGetValue(audio.Tag, out var bin))
            bin.Add(audio.Barcode);
    }

    public static AudioBin CreateBin(string tag, params string[] fallbacks)
    {
        if (!tag.StartsWith(TagPrefix, StringComparison.Ordinal))
            throw new ArgumentException($"Tag must start with {TagPrefix}", nameof(tag));

        var clippedTag = tag[TagPrefix.Length..];
        if (Bins.TryGetValue(clippedTag, out var bin))
            return bin;

        var newBin = new AudioBin(clippedTag, fallbacks.ToList());
        if (_isRegistered)
            PopulateBin(newBin);
        Bins.Add(clippedTag, newBin);
        return newBin;
    }

    public static void RegisterPallet(Pallet pallet)
    {
        foreach (var palletDataCard in pallet.DataCards)
        {
            if (palletDataCard.GetIl2CppType() != MonoDiscType)
                continue;

            if (!palletDataCard._description.StartsWith(TagPrefix, StringComparison.Ordinal))
                continue;

            var tag = palletDataCard._description[TagPrefix.Length..];
            if (tag == null)
                continue;

            var registerableAudio = new RegisterableAudio(tag, palletDataCard._barcode._id);
            BinAudio(registerableAudio);
            RegisterableAudios.Add(registerableAudio);
        }
    }

    public static void RegisterAll()
    {
        if (_isRegistered)
            return;
        _isRegistered = true;

        foreach (var pallet in AssetWarehouse.Instance.GetPallets())
        {
            RegisterPallet(pallet);
        }
    }
}