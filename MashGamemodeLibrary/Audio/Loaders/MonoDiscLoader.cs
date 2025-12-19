using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Marrow;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Loaders;

public class MonoDiscLoader : IAudioLoader
{
    public MonoDiscLoader(IEnumerable<string> monoDiscs)
    {
        AudioNames = monoDiscs.ToList();
    }

    public MonoDiscLoader(IEnumerable<MonoDiscReference> monoDiscs)
    {
        AudioNames = monoDiscs.Select(v => v._barcode.ID).ToList();
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