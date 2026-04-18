#if DEBUG
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace MashGamemodeLibrary.Debug;

public class ShowDebugKeybind : DebugKeybind
{

    protected override KeyCode _key => KeyCode.N;
    protected override Action _onPress { get; } = () =>
    {
        DebugRenderer.IsEnabled = !DebugRenderer.IsEnabled;

        InternalLogger.Debug("Debgug Renderer " + (DebugRenderer.IsEnabled ? "Enabled" : "Disabled"));
    };
}
#endif