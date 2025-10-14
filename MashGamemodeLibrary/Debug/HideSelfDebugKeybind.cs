using LabFusion.Player;
using MashGamemodeLibrary.Vision;
using MelonLoader;
using UnityEngine;

#if DEBUG
namespace MashGamemodeLibrary.Debug;

public class HideSelfDebugKeybind : DebugKeybind
{
    protected override KeyCode _key => KeyCode.H;

    protected override Action _onPress => () =>
    {
        MelonLogger.Msg("Toggling self visibility");
        PlayerIDManager.LocalID.SetHidden("test", !PlayerIDManager.LocalID.IsHidden());
    };
}
#endif