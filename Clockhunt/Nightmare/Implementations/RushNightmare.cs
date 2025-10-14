using Clockhunt.Vision;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Player;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player;
using UnityEngine;

namespace Clockhunt.Nightmare.Implementations;

public class RushNightmareInstance : NightmareInstance
{
    private const float MaxVelocity = 50f;
    private const float Acceleration = 300f;
    private bool _grip;

    public RushNightmareInstance(byte owner, RushNightmareDescriptor descriptor) : base(owner, descriptor)
    {
    }

    public override void OnApplied()
    {
        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            VisionManager.EnableNightVision();

            Owner.RigRefs.RigManager.remapHeptaRig.doubleJump = true;
        });
    }

    public override void OnRemoved()
    {
        Executor.RunIfMe(Owner.PlayerID, () => { Owner.RigRefs.RigManager.remapHeptaRig.doubleJump = false; });
    }

    public override void OnUpdate(float delta)
    {
        if (!_grip)
            return;

        if (!Owner.HasRig)
            return;

        var rightHand = Owner.RigRefs.RightHand.transform;

        var forward = rightHand.forward;
        var ray = new Ray(rightHand.position + Vector3.forward, forward);

        var target = rightHand.position + forward * 30f;
        if (Physics.Raycast(ray, out var hit, 30f)) target = hit.point;

        var source = rightHand.position;
        var direction = (target - source).normalized;

        var currentVelocity = Owner.RigRefs.RigManager.physicsRig.rbFeet.velocity;

        var difference = Mathf.Max((Vector3.Dot(currentVelocity.normalized, direction) - 1) * 2f, 1);

        var distance = 1.0f - Mathf.Clamp01(currentVelocity.magnitude / MaxVelocity);

        var acceleration = direction * Acceleration * delta * distance * difference;

        Owner.RigRefs.RigManager.physicsRig.rbFeet.velocity += acceleration;
    }

    public override void OnPlayerAction(PlayerID playerID, PhaseAction action, Handedness handedness)
    {
        if (!playerID.IsMe)
            return;

        if (handedness != Handedness.RIGHT) return;

        switch (action)
        {
            case PhaseAction.HandClose:
                _grip = true;
                break;
            case PhaseAction.HandOpen:
                _grip = false;
                break;
        }
    }
}

public class RushNightmareDescriptor : NightmareDescriptor
{
    public override string Name => "Rush";
    public override string HunterDescription => "Run into your enemies.";
    public override string SurvivorDescription => "Use cover!";
    public override string Avatar => "nitwit.TF2AvatarPack.Avatar.HeavyAPose";
    public override int Weight => 10;
    public override float AbilityCooldown => 60f;

    public override PlayerStats Stats => new()
    {
        Vitality = 6f,
        UpperStrength = 8f,
        Speed = 3f,
        Agility = 3f,
        LowerStrength = 8f
    };

    public override NightmareInstance CreateInstance(byte ownerID)
    {
        return new RushNightmareInstance(ownerID, this);
    }
}