using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Marrow;
using MashGamemodeLibrary.Audio.Registry;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Loaders;

public class AudioBinLoader : IAudioLoader
{
    private readonly AudioBin _audioBin;
    
    public AudioBinLoader(AudioBin audioBin)
    {
        _audioBin = audioBin;
    }

    public IReadOnlyList<string> AudioNames => _audioBin.GetAll();

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