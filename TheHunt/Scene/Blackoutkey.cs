using MashGamemodeLibrary.Debug;
using UnityEngine;

namespace TheHunt.Scene;

public class Blackoutkey : DebugKeybind
{

    protected override KeyCode _key { get; } = KeyCode.K;
    protected override Action _onPress { get; } = () =>
    {
        LightManager.SetBlackout(!LightManager.IsBlackout);
    };
}