using Clockhunt.Config;
using Clockhunt.Entities.Tags;
using Clockhunt.Game;
using Clockhunt.Vision;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player;
using UnityEngine;

namespace Clockhunt.Nightmare.Implementations;

public class SkinwalkerNightmareInstance : NightmareInstance
{
    private const string NightmareAvatarBarcode = "Random.OWNTeamAvatars.Avatar.Wendigo"; // Example barcode, replace with actual
    private string _disguiseAvatarBarcode = NightmareAvatarBarcode; // Example barcode, replace with actual
    private bool _isDisguised = true;
    
    public SkinwalkerNightmareInstance(byte owner, NightmareDescriptor descriptor) : base(owner, descriptor)
    {
    }

    public override bool CanGrab(GrabData grab)
    {
        if (!grab.IsHoldingItem(out var item)) return true;
        var entity = item.NetworkEntity;
        return (entity != null && entity.HasTag<ClockMarker>()) || base.CanGrab(grab);
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
            WinStateManager.OverwriteLives(0);
        });
    }

    public override void OnAbilityKeyTapped(Handedness handedness)
    {
        if (_isDisguised)
        {
            PlayerStatManager.SetAvatarAndStats(NightmareAvatarBarcode, Descriptor.Stats);
        }
        else
        {
            PlayerStatManager.SetAvatarAndStats(_disguiseAvatarBarcode, Descriptor.GetStats());
        }
        
        _isDisguised = !_isDisguised;
    }
}

public class SkinwalkerNightmareDescriptor : NightmareDescriptor
{
    public  override string Name => "Skinwalker";
    public  override string HunterDescription => "Player lives have been reduced to 0. Kill them all before they escape.";
    public  override string SurvivorDescription => "You have no lives. Work with your team to escape or eliminate the nightmare.";
    public override string? Avatar => null;
    public override bool RegenerateHealth => true;
    public override int Weight => 4;
    public override float AbilityCooldown => 10f;

    public override PlayerStats Stats => new()
    {
        Vitality = 2.5f,
        UpperStrength = 5f,
        Speed = 2f,
        Agility = 2f,
        LowerStrength = 5f
    };

    public override PlayerStats GetStats()
    {
        return ClockhuntConfig.DefaultStats;
    }

    public override NightmareInstance CreateInstance(byte ownerID)
    {
        return new SkinwalkerNightmareInstance(ownerID, this);
    }
}