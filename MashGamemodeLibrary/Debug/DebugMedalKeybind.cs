#if DEBUG
using MashGamemodeLibrary.Integrations;
using UnityEngine;

namespace MashGamemodeLibrary.Debug;

public class DebugMedalKeybind : DebugKeybind
{

    protected override KeyCode _key => KeyCode.I;
    protected override Action _onPress => () =>
    {
        MedalIntegration.SendEvent(new MedalEvent()
        {
            EventId = "debug_medal",
            EventName = "Debug Medal",
        });
    };
}
#endif