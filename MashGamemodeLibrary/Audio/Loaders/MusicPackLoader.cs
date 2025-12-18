using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Marrow;
using MashGamemodeLibrary.Audio.Music;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Loaders;

public class MusicPackLoader: IAudioLoader
{
    public MusicPackLoader(MusicPackTags tag)
    {
        AudioNames = MusicPackManager.GetTracks(tag);
    }
    
    public List<string> AudioNames { get; }

    public void RefreshNames()
    {
        // No-op
    }

    public bool IsLoading { get; private set; }

    public void Load(string name, Action<AudioClip?> onLoaded)
    {
        IsLoading = true;

        // This technically does load any monodisc you give it.
        var monoDiscReference = new MonoDiscReference(name);
        var audioReference = new AudioReference(monoDiscReference);

        audioReference.LoadClip(audioClip =>
        {
            IsLoading = false;
            onLoaded.Invoke(audioClip);
        });
    }
}