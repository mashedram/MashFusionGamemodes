using UnityEngine;

namespace MashGamemodeLibrary.Audio.Loaders;

public interface IAudioLoader
{
    List<string> AudioNames { get; }

    bool IsLoading { get; }
    void RefreshNames();
    void Load(string name, Action<AudioClip?> onLoaded);
}