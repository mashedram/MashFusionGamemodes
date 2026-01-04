using Clockhunt.Config;
using Clockhunt.Entities.Tags;
using Clockhunt.Game;
using Clockhunt.Nightmare.Config;
using Clockhunt.Vision;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Stats;

namespace Clockhunt.Nightmare.Implementations;

public class SkinwalkerNightmareInstance : NightmareInstance
{
    private const string
        NightmareAvatarBarcode = "Random.OWNTeamAvatars.Avatar.Wendigo";

    private string _disguiseAvatarBarcode = NightmareAvatarBarcode;
    private bool _isDisguised = true;

    public SkinwalkerNightmareInstance(byte owner, NightmareDescriptor descriptor) : base(owner, descriptor)
    {
    }

    public override bool CanGrab(GrabData grab)
    {
        return _isDisguised || base.CanGrab(grab);
    }

    public override bool CanStartTensionMusic(NetworkPlayer nightmare, float distance, bool lineOfSight)
    {
        return false;
    }

    public override bool CanStartChaseMusic(NetworkPlayer nightmare, float distance, bool lineOfSight)
    {
        return _isDisguised == false && base.CanStartChaseMusic(nightmare, distance, lineOfSight);
    }

    public override void OnApplied()
    {
        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            VisionManager.EnableNightVision();
            _disguiseAvatarBarcode = LocalAvatar.AvatarBarcode ?? NightmareAvatarBarcode;
            _isDisguised = true;
        });
        Executor.RunIfHost(() =>
        {
            foreach (var networkPlayer in NetworkPlayer.Players)
            {
                if (networkPlayer.TryGetComponent<LimitedRespawnComponent>(out var tag))
                {
                    tag.SetRespawns(0);
                }
            }
        });
    }

    public override void OnAbilityKeyTapped(Handedness handedness)
    {
        if (_isDisguised)
            PlayerStatManager.SetAvatarAndStats(GetConfig<NightmareConfig>().AvatarOverride ?? NightmareAvatarBarcode, Descriptor.Stats);
        else
            PlayerStatManager.SetAvatarAndStats(_disguiseAvatarBarcode, Descriptor.GetStats());

        _isDisguised = !_isDisguised;
    }
}

public class SkinwalkerNightmareDescriptor : NightmareDescriptor
{
    public override string Name => "Skinwalker";

    public override string HunterDescription =>
        "Player lives have been reduced to 0. Kill them all before they escape.";

    public override string SurvivorDescription =>
        "You have no lives. Work with your team to escape or eliminate the nightmare.";

    public override string? Avatar => null;
    public override bool RegenerateHealth => true;

    public override Func<NightmareConfig> ConfigFactory => () => new NightmareConfig
    {
        AbilityCooldown = 10f
    };

    public override PlayerStats Stats => new()
    {
        Vitality = 8f,
        UpperStrength = 5f,
        Speed = 2f,
        Agility = 2f,
        LowerStrength = 5f
    };

    public override PlayerStats GetStats()
    {
        return Clockhunt.Config.DefaultStats;
    }

    public override NightmareInstance CreateInstance(byte ownerID)
    {
        return new SkinwalkerNightmareInstance(ownerID, this);
    }
}