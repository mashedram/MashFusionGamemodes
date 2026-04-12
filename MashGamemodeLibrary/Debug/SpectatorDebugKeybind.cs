#if DEBUG
using LabFusion.Player;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Spectating;
using MelonLoader;
using UnityEngine;

namespace MashGamemodeLibrary.Debug;

public class SpectatorDebugKeybind : DebugKeybind
{
    protected override KeyCode _key => KeyCode.J;

    protected override Action _onPress => () =>
    {
        MelonLogger.Msg("Toggling spectate for all other players");
        Executor.RunIfHost(() =>
        {
            var playerID = PlayerIDManager.LocalID;
            playerID.SetSpectating(!playerID.IsSpectating());
        });
    };
}
#endif