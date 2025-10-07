using UnityEngine;

namespace MashGamemodeLibrary.Audio.Players.Extensions;

public interface IPositionalAudioPlayer
{
    public Vector3? Position { get; }
}