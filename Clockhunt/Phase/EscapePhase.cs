using MashGamemodeLibrary.Phase;

namespace Clockhunt.Phase;

public class EscapePhase : GamePhase
{
    public override string Name => "Escape";
    public override float Duration => 300f;
    
    protected override void OnPhaseEnter()
    {
        // TODO: Start Playing an Alarm Sound once a player enters the pickup zone
        // TODO: Once the pickup is open, start playing radio static sound
        // TODO: Clear all remaining lives
        
        // TODO: Make the pickup point one of the clock hiding spots
    }

    protected override void OnUpdate()
    {
        // TODO: Check if player is in pickup zone
        // TODO: Survivor wins if they stay in the zone for 30 seconds
    }
}