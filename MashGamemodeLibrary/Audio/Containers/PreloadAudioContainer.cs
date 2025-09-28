using MashGamemodeLibrary.Audio.Loaders;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MashGamemodeLibrary.Audio.Containers;

public class PreloadAudioContainer : IAudioContainer
{
    private readonly IAudioLoader _loader;
    private readonly Dictionary<string, AudioClip> _clips = new();

    public PreloadAudioContainer(IAudioLoader loader)
    {
        _loader = loader;
        
        PreloadAll();
    }
    
    public List<string> AudioNames => _loader.AudioNames;

    public bool IsLoading { get; private set; }

    public void PreloadAll()
    {
        IsLoading = true;
        
        foreach (var oldClip in _clips.Values)
        {
            oldClip.UnloadAudioData();
            Object.Destroy(oldClip);
        }
        
        _clips.Clear();

        var toLoad = AudioNames.Count;
        foreach (var name in AudioNames)
        {
            _loader.Load(name, audioClip =>
            {
                toLoad -= 1;
                if (!audioClip)
                {
                    MelonLogger.Error($"Failed to preload audio clip: {name}");
                    return;
                }
                
                _clips[name] = audioClip;
                if (toLoad <= 0)
                {
                    IsLoading = false;
                }
            });
        }
    }

    public void RequestClip(string name, Action<AudioClip?> onClipReady)
    {
        if (IsLoading)
        {
            MelonLogger.Warning("AudioContainer is not fully loaded, clips may be missing.");
        }
        
        onClipReady.Invoke(_clips.GetValueOrDefault(name));
    }
}