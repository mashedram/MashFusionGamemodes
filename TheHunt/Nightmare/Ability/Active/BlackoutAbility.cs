using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Audio.Players.Callers;
using MashGamemodeLibrary.Entities.ECS.Attributes;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Gamemode;
using TheHunt.Teams;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheHunt.Nightmare.Ability.Active;

public class BlackoutAbility : IActiveAbility
{
    public Handedness Handedness => Handedness.LEFT;

    public string Description => "Scream to mark enemies";

    public void UseAbility(Nightmare nightmare, NetworkPlayer networkPlayer)
    {
        if (!networkPlayer.HasRig)
            return;
        
        if (!networkPlayer.PlayerID.IsMe)
            return;

        var targetHider = NetworkPlayer.Players
            .Where(p => p.PlayerID.IsValid && !p.IsSpectating() && p.PlayerID.IsTeam<HiderTeam>())
            .MaxBy(p => p.DistanceSqr);
        
        targetHider?.TryAddComponent(() => new ScreamMarker());

        var position = networkPlayer.RigRefs.Head.position;
        TheHuntContext.RoarAudioPlayer.PlayRandom(position);
    }
    public float Cooldown => 30f;

    public void OnAdded(NetworkPlayer networkPlayer)
    {
        // NO-OP
    }
    
    public void OnRemoved(NetworkPlayer networkPlayer)
    {
        // NO-OP
    }
}