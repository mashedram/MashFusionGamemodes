using LabFusion.Player;
using MashGamemodeLibrary.Player.Team;

namespace MashGamemodeLibrary.Player.Data.Events.Data;

public record TeamChangedEvent(PlayerID PlayerID, LogicTeam? Team) : IPlayerEvent;