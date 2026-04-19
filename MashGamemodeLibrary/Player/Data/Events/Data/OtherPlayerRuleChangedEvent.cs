using LabFusion.Entities;
using LabFusion.Player;
using MashGamemodeLibrary.Player.Spectating.data.Rules;

namespace MashGamemodeLibrary.Player.Data.Events.Data;

public record OtherPlayerRuleChangedEvent(PlayerID Player, IPlayerRule Rule) : IPlayerEvent;