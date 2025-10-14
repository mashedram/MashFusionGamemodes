using MashGamemodeLibrary.Audio.Loaders;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Containers;

public class LoadOnDemandContainer : IAudioContainer
{
    private readonly IAudioLoader _loader;

    public LoadOnDemandContainer(IAudioLoader loader)
    {
        _loader = loader;
    }

    public List<string> AudioNames => _loader.AudioNames;

    public bool IsLoading => _loader.IsLoading;

    public void RequestClip(string name, Action<AudioClip?> onClipReady)
    {
        _loader.Load(name, onClipReady);
    }
}