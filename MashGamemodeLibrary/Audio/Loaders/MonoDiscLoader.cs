using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Marrow;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Loaders;

public class MonoDiscLoader : IAudioLoader
{
    public List<string> AudioNames { get; }
    
    public MonoDiscLoader(string[] monoDiscs)
    {
        AudioNames = monoDiscs.ToList();
    }

    public void RefreshNames()
    {
        // No-op
    }
    
    public bool IsLoading { get; private set; }
    
    public void Load(string name, Action<AudioClip?> onLoaded)
    {
        IsLoading = true;
        
        var monoDiscReference = new MonoDiscReference(name);
        var audioReference = new AudioReference(monoDiscReference);

        audioReference.LoadClip(audioClip =>
        {
            IsLoading = false;
            onLoaded.Invoke(audioClip);
        });
    }
}