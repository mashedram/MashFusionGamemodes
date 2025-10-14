using AudioImportLib;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Loaders;

public class AudioFileLoader : IAudioLoader
{
    private static readonly string RootPath = Path.Combine(MelonEnvironment.UserDataDirectory, "Audio");
    private readonly string _audioDirectoryPath;
    private readonly Dictionary<string, string> _nameToPath = new();

    public AudioFileLoader(string subdirectory)
    {
        _audioDirectoryPath = Path.Combine(RootPath, subdirectory);
        if (!Directory.Exists(_audioDirectoryPath))
            Directory.CreateDirectory(_audioDirectoryPath);

        RefreshNames();
    }

    public void RefreshNames()
    {
#if DEBUG
        MelonLogger.Msg($"Fetching audio list from directory: {_audioDirectoryPath}");
#endif
        var files = Directory.GetFiles(_audioDirectoryPath);
        foreach (var file in files)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            _nameToPath.TryAdd(name, file);

#if DEBUG
            MelonLogger.Msg($"Found audio file: {name} at path: {file}");
#endif
        }
    }

    public List<string> AudioNames => _nameToPath.Keys.ToList();

    public bool IsLoading { get; private set; }

    public void Load(string name, Action<AudioClip?> onLoaded)
    {
        IsLoading = true;

        if (!_nameToPath.TryGetValue(name, out var path))
        {
            IsLoading = false;
            onLoaded.Invoke(null);
            return;
        }

#if DEBUG
        MelonLogger.Msg($"Loading audio clip from path: {path}");
#endif
        var clip = API.LoadAudioClip(path);

        onLoaded.Invoke(clip);
    }
}