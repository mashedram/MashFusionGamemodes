using LabFusion.Extensions;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Containers;

public class DesyncedAudioContainer : ISyncedAudioContainer
{
    private readonly IAudioContainer _parent;

    public DesyncedAudioContainer(IAudioContainer parent)
    {
        _parent = parent;
    }

    public IReadOnlyList<string> AudioNames => _parent.AudioNames;
    public bool IsLoading => _parent.IsLoading;
    public void RequestClip(string name, Action<AudioClip?> onClipReady)
    {
        _parent.RequestClip(name, onClipReady);
    }

    public List<ulong> AudioHashes { get; } = new()
    {
        1
    };
    public ulong? GetAudioHash(string name)
    {
        return 1;
    }
    public void RequestClip(ulong hash, Action<AudioClip?> onClipReady)
    {
        var name = AudioNames.GetRandom();
        RequestClip(name, onClipReady);
    }
}