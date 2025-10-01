using UnityEngine;

namespace MashGamemodeLibrary.Audio.Containers;

public interface ISyncedAudioContainer : IAudioContainer
{
    List<ulong> AudioHashes { get; }
    ulong? GetAudioHash(string name);
    void RequestClip(ulong hash, Action<AudioClip?> onClipReady);
}