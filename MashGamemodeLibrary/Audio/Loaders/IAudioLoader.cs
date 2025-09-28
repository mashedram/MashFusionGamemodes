using UnityEngine;

namespace MashGamemodeLibrary.Audio.Loaders;

public interface IAudioLoader
{
    void RefreshNames();
    List<string> AudioNames { get; }
    
    bool IsLoading { get; }
    void Load(string name, Action<AudioClip?> onLoaded);
}