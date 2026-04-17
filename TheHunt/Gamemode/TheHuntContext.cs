using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Environment;
using TheHunt.Audio;

namespace TheHunt.Gamemode;

public class TheHuntContext : GameModeContext<TheHuntContext>
{
    public readonly EnvironmentManager<TheHuntContext, EnvironmentContext> EnvironmentPlayer =
        new(EnvironmentContext.GetContext);
}