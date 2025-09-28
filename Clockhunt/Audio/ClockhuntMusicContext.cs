using MashGamemodeLibrary.Phase;

namespace Clockhunt.Audio;

public class ClockhuntMusicContext
{
    private GamePhase Phase;
    
    public bool IsPhase<T>() where T : GamePhase
    {
        return Phase is T;
    }
    
    public static ClockhuntMusicContext GetContext(ClockhuntContext context)
    {
        return new ClockhuntMusicContext
        {
            Phase = context.PhaseManager.GetActivePhase()
        };
    }
}