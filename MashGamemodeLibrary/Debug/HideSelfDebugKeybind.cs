using LabFusion.Player;
using MashGamemodeLibrary.Vision;
using UnityEngine;

#if DEBUG
namespace MashGamemodeLibrary.Debug;

public class HideSelfDebugKeybind : DebugKeybind
{
    protected override KeyCode _key => KeyCode.H;
    protected override Action _onPress => () =>
    {
        MelonLoader.MelonLogger.Msg("Toggling self visibility");
        PlayerIDManager.LocalID.ForceHide(!PlayerIDManager.LocalID.IsHidden());
    };
}
#endif