using MashGamemodeLibrary.Audio.Players.Basic;
using MashGamemodeLibrary.Audio.Players.Basic.Providers;
using MashGamemodeLibrary.Audio.Players.Object;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Modifiers;

/// <summary>
/// A locally ran modifier that can modify an AudioSource over time.
///
/// Create one instance per AudioSource.
/// </summary>
public interface IAudioModifier
{
    public void OnStart(ref AudioSource source);
    public void Update(ref AudioSource source, float delta) {}
}