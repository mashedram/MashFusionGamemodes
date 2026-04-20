using MashGamemodeLibrary.Audio.Players.Extensions;
using MashGamemodeLibrary.Audio.Players.Object;
using MashGamemodeLibrary.Environment.Effector;
using MashGamemodeLibrary.Player.Team;
using UnityEngine;

namespace TheHunt.Audio.Effectors.Stinger;

public class StingerAudioEffector : CustomAudioEffector<EnvironmentContext, AnnouncementAudioPlayer>
{

    public StingerAudioEffector() : base(Gamemode.TheHunt.Context.StingerAudioPlayer)
    {
    }
    
    public override Enum Track => EffectorTracks.Stinger;

    protected override void Play(AnnouncementAudioPlayer audioPlayer, EnvironmentContext context)
    {
        audioPlayer.PlayRandom();
    }
}