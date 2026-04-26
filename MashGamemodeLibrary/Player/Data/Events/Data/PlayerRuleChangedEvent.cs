using LabFusion.Player;
using MashGamemodeLibrary.Player.Data.Rules;

namespace MashGamemodeLibrary.Player.Data.Events.Data;

public record PlayerRuleChangedEvent(PlayerID Player, IPlayerRule Rule) : IPlayerEvent;