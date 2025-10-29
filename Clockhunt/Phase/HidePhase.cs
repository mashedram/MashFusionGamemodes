using Clockhunt.Config;
using Clockhunt.Entities;
using Clockhunt.Game;
using Clockhunt.Game.Player;
using Clockhunt.Game.Teams;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;
using UnityEngine;

namespace Clockhunt.Phase;

public class HidePhase : GamePhase, ITimedPhase
{
    public override string Name => "Hide";
    public override float Duration => Clockhunt.Config.HidePhaseDuration;

    public override PhaseIdentifier GetNextPhase()
    {
        if (!HasReachedDuration()) return PhaseIdentifier.Empty();
        
        return PhaseIdentifier.Of<HuntPhase>();
    }

    protected override void OnPhaseEnter()
    {
        Notifier.Send(new Notification
        {
            Title = Clockhunt.Config.GameType switch
            {
                GameType.Clockhunt => "Clockhunt",
                GameType.HideAndSeek => "Hide and Seek",
                _ => throw new ArgumentOutOfRangeException()
            },
            Message = Clockhunt.Config.GameType switch
            {
                GameType.Clockhunt => "Hide the clocks well",
                GameType.HideAndSeek => "Hide and don't get found",
                _ => throw new ArgumentOutOfRangeException()
            },
            PopupLength = 4f,
            SaveToMenu = false,
            ShowPopup = true,
            Type = NotificationType.INFORMATION
        });
        
        Executor.RunIfHost(() =>
        {
            TeamManager.AssignAll<SurvivorTeam>();

            if (Clockhunt.Config.GameType == GameType.Clockhunt)
            {
                NetworkPlayer.Players.ForEach(player =>
                {
                    if (!player.HasRig) return;

                    // TODO: Make this spawn a new one once the old one was placed
                    for (var i = 0; i < Clockhunt.Config.ClocksPerPlayer; i++)
                        ClockManager.SpawnEntityForPlayer(player);
                });
            }
        });

        if (Clockhunt.Config.RuntimeSpawnPointsEnabled) SpawnManager.Reset();
    }

    protected override void OnUpdate()
    {
        if (Clockhunt.Config.RuntimeSpawnPointsEnabled) SpawnManager.Update(Time.deltaTime);
    }
}