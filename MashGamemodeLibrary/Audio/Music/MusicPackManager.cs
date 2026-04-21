using BoneLib;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Marrow;
using MashGamemodeLibrary.Audio.Registry;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Audio.Music;

public enum MusicPackTags
{
    Combat,
    Ambient,
    Intermission
}

public static class MusicPackManager
{
    private static readonly Dictionary<MusicPackTags, AudioBin> AudioBins = new()
    {
        {
            MusicPackTags.Combat, AudioRegistry.CreateBin("MashTags.MusicPack.Combat",
                FusionMonoDiscReferences.CombatSongReferences.Select(s => s._barcode._id).Union(new[]
                {
                    "SLZ.BONELAB.Content.MonoDisc.LavaGangLoop",
                    "SLZ.BONELAB.Content.MonoDisc.FistFightFugue",
                    "SLZ.BONELAB.Content.MonoDisc.Bonestep",
                    "SLZ.BONELAB.Content.MonoDisc.BackThroughTheLab",
                    "SLZ.BONELAB.Content.MonoDisc.HallVortex",
                    "SLZ.BONELAB.Content.MonoDisc.RipToBits"
                }).ToArray())
        },
        {
            MusicPackTags.Ambient, AudioRegistry.CreateBin("MashTags.MusicPack.Ambient", new[]
            {
                FusionMonoDiscReferences.FistfightFusionReference._barcode._id,
                "SLZ.BONELAB.Content.MonoDisc.MirrorAlive",
                "SLZ.BONELAB.Content.MonoDisc.UnsolvedSciences",
                "SLZ.BONELAB.Content.MonoDisc.EverydayCastle",
                "SLZ.BONELAB.Content.MonoDisc.BonelightSonata"
            })
        },
        {
            MusicPackTags.Intermission, AudioRegistry.CreateBin("MashTags.MusicPack.Intermission", new[]
            {
                "SLZ.BONELAB.Content.MonoDisc.EverydayCastle"
            })
        }
    };

    private static readonly Dictionary<MusicPackTags, List<string>> TagToDiscs = new();

    public static IReadOnlyList<string> GetTracks(MusicPackTags tag)
    {
        return AudioBins.GetValueOrDefault(tag)?.GetAll() ?? Array.Empty<string>();
    }
}