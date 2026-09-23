using LabFusion.Entities;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.Interaction.Grabbing;
using MashGamemodeLibrary.Player.Team;

namespace TheHunt.Nightmare.Ability.Passive;

public class KillOnTouch : IAbility, IGrabCallback, IPlayerAttached
{
    private NetworkPlayer? _player;
    public void OnReady(NetworkPlayer networkPlayer)
    {
        _player = networkPlayer;
    }
    
    public void OnGrabbed(GrabRequest grabRequest)
    {
        if (grabRequest.GrabbedNetworkEntity == null)
            return;
        
        if (NetworkPlayerManager.TryGetPlayer(grabRequest.GrabbedEntity, out var player))
            return;
            
        if (player.PlayerID.IsTeamMember())
            return;
        
        player.RigRefs.Health.TAKEDAMAGE(1000f);
    }
}