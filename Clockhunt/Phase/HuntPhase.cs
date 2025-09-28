using Clockhunt.Nightmare.Implementations;
using LabFusion.Entities;
using MashGamemodeLibrary.Phase;

namespace Clockhunt.Phase;

public class HuntPhase : GamePhase
{
    public override string Name => "Hunt";
    public override float Duration => 300f;

    protected override void OnPhaseEnter()
    {
        var context = Clockhunt.GetContext();
        
        // TODO: Select a few players instead
        foreach (var networkPlayer in NetworkPlayer.Players)
        {
            context.NightmareManager.SetNightmare<EntityNightmareDescriptor>(networkPlayer);
        }
    }
}