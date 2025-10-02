#if DEBUG
using LabFusion.Entities;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Spectating;
using MelonLoader;
using UnityEngine;

namespace MashGamemodeLibrary.Debug;

public class SpectatorDebugKeybind : DebugKeybind
{
    protected override KeyCode _key => KeyCode.P;

    protected override Action _onPress => () =>
    {
        MelonLogger.Msg("Toggling spectate for all other players");
        Executor.RunIfHost(() =>
        {
            foreach (var playerID in from networkPlayer in NetworkPlayer.Players where networkPlayer.PlayerID.IsMe select networkPlayer.PlayerID)
            {
                playerID.SetSpectating(!SpectatorManager.IsPlayerSpectating(playerID.SmallID));
            }
        });
    };
}
#endif