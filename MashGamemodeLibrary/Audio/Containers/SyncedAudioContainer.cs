using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Util;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering.Universal.LibTessDotNet;
using Object = Il2CppSystem.Object;

namespace MashGamemodeLibrary.Audio.Containers;

public class SyncedAudioContainer : ISyncedAudioContainer
{
    private readonly IAudioLoader _loader;
    private readonly Dictionary<string, ulong> _nameToHash = new();
    private readonly Dictionary<ulong, AudioClip> _clips = new();

    public SyncedAudioContainer(IAudioLoader loader)
    {
        _loader = loader;
        
        PreloadAll();
    }
    
    public List<string> AudioNames => _loader.AudioNames;
    public List<ulong> AudioHashes => _clips.Keys.ToList();

    public bool IsLoading { get; private set; }

    private void PreloadAll()
    {
        IsLoading = true;
        
        foreach (var oldClip in _clips.Values)
        {
            oldClip.UnloadAudioData();
            UnityEngine.Object.Destroy(oldClip);
        }
        
        _clips.Clear();

        var names = _loader.AudioNames;
        var toLoad = names.Count;
        foreach (var name in names)
        {
            _loader.Load(name, audioClip =>
            {
                toLoad -= 1;
                if (!audioClip)
                {
                    MelonLogger.Error($"Failed to preload audio clip: {name}");
                    return;
                }

                var identifier = name.GetStableHash();
                _clips[identifier] = audioClip!;
                _nameToHash[name] = identifier;
                if (toLoad <= 0)
                {
                    IsLoading = false;
                }
            });
        }
    }

    public ulong? GetAudioHash(string name)
    {
        return _nameToHash.GetValueOrDefault(name);
    }

    public void RequestClip(string name, Action<AudioClip?> onClipReady)
    {
        var hash = GetAudioHash(name);
        if (hash != null) return;
        onClipReady.Invoke(null);
    }

    public void RequestClip(ulong identifier, Action<AudioClip?> onClipReady)
    {
        if (IsLoading)
        {
            MelonLogger.Warning("AudioContainer is not fully loaded, clips may be missing.");
        }
        
        onClipReady.Invoke(_clips.GetValueOrDefault(identifier));
    }
}