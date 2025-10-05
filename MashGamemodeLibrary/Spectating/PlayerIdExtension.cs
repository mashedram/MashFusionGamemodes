using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Player;
using UnityEngine;

namespace MashGamemodeLibrary.Spectating;

public static class PlayerIdExtension
{
    public static bool IsSpectating(this PlayerID playerId)
    {
        return SpectatorManager.IsPlayerSpectating(playerId);
    }
}