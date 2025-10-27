using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.Entities.Extenders;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Spectating;
using MashGamemodeLibrary.Vision;
using TeamManager = MashGamemodeLibrary.Player.Team.TeamManager;

namespace MashGamemodeLibrary.Phase.Rounds;

public abstract class RoundContext
{
    public abstract int RoundCount { get; }
    public abstract float RoundCooldown { get; }
}