using Clockhunt.Vision;
using LabFusion.Entities;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Vision;

namespace Clockhunt.Nightmare.Implementations;

public class StalkerNightmareInstance : NightmareInstance
{
    private const float VisibilityDistance = 4f;
    private const string StalkerHideKey = "stalker";

    public StalkerNightmareInstance(byte owner, StalkerNightmareDescriptor descriptor) : base(owner, descriptor)
    {
    }

    public override void OnApplied()
    {
        Executor.RunIfMe(Owner.PlayerID, VisionManager.EnableNightVision);
    }

    public override bool CanStartChaseMusic(NetworkPlayer nightmare, float distance, bool lineOfSight)
    {
        return distance < VisibilityDistance;
    }

    public override void OnUpdate(float delta)
    {
        if (Owner.PlayerID.IsMe)
            return;

        var headPosition = Clockhunt.Context.LocalPlayer.RigRefs.Head.transform.position;

        var toHead = headPosition - Owner.RigRefs.Head.transform.position;
        var distance = toHead.magnitude;

        var isHidden = distance > VisibilityDistance;

        Owner.PlayerID.SetHidden(StalkerHideKey, isHidden);
    }

    public override void OnRemoved()
    {
        Owner.PlayerID.SetHidden(StalkerHideKey, false);
    }
}

public class StalkerNightmareDescriptor : NightmareDescriptor
{
    public override string Name => "Stalker";
    public override string HunterDescription => "You are hidden when far away from survivors.";
    public override string SurvivorDescription => "Watch your back.";
    public override string Avatar => "Random.OWNTeamAvatars.Avatar.Creature";
    public override int Weight => 8;
    public override float AbilityCooldown => 60f;

    public override PlayerStats Stats => new()
    {
        Vitality = 2f,
        UpperStrength = 8f,
        Speed = 3f,
        Agility = 1f,
        LowerStrength = 8f
    };

    public override NightmareInstance CreateInstance(byte ownerID)
    {
        return new StalkerNightmareInstance(ownerID, this);
    }
}