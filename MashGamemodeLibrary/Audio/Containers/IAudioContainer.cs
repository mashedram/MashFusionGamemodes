using UnityEngine;

namespace MashGamemodeLibrary.Audio.Containers;

public interface IAudioContainer
{
    List<string> AudioNames { get; }
    bool IsLoading { get; }
    void RequestClip(string name, Action<AudioClip?> onClipReady);
}