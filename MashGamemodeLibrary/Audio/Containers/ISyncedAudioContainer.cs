using UnityEngine;

namespace MashGamemodeLibrary.Audio.Containers;

public interface ISyncedAudioContainer : IAudioContainer
{
    List<int> AudioHashes { get; }
    int? GetAudioHash(string name);
    void RequestClip(int hash, Action<AudioClip?> onClipReady);
}