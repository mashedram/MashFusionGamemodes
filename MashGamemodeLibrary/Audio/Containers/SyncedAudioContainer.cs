using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Util;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MashGamemodeLibrary.Audio.Containers;

public class SyncedAudioContainer : ISyncedAudioContainer
{
    private readonly IAudioLoader _loader;
    private readonly Dictionary<string, ulong> _nameToHash = new();
    private readonly Dictionary<ulong, string> _hashToName = new();
    private readonly Dictionary<ulong, AudioClip?> _clipCache = new();
    
    public SyncedAudioContainer(IAudioLoader loader)
    {
        _loader = loader;

        PreloadAll();
    }

    public List<string> AudioNames => _loader.AudioNames;
    public List<ulong> AudioHashes => _clipCache.Keys.ToList();

    public bool IsLoading { get; private set; }
    
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

    public void RequestClip(ulong hash, Action<AudioClip?> onClipReady)
    {
        if (_clipCache.TryGetValue(hash, out var cachedClip))
            onClipReady.Invoke(cachedClip);

        var name = _hashToName[hash];
        _loader.Load(name, clip =>
        {
            _clipCache[hash] = clip;
            onClipReady.Invoke(clip);
        });
    }

    private void PreloadAll()
    {
        IsLoading = true;

        foreach (var oldClip in _clipCache.Values)
        {
            if (oldClip == null)
                continue;
            
            oldClip.UnloadAudioData();
            Object.Destroy(oldClip);
        }

        _clipCache.Clear();

        var names = _loader.AudioNames;
        foreach (var name in names)
        {
            var hash = name.GetStableHash();
            _nameToHash[name] = hash;
            _hashToName[hash] = name;
        }
    }
}