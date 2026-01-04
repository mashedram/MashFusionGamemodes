using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using MashGamemodeLibrary.Entities.Behaviour;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;

namespace MashGamemodeLibrary.Player.Actions;

public enum PlayerInputType
{
    Ability,
    Grip
}

public static class PlayerActionManager
{
    private static readonly Dictionary<Handedness, bool> LastGripStateMap = new();

    private static readonly IBehaviourCache<IPlayerActionCallback> PlayerActionTags = EcsManager.CreateBehaviorCache<IPlayerActionCallback>();
    private static readonly IBehaviourCache<IPlayerInputCallback> PlayerInputTags = EcsManager.CreateBehaviorCache<IPlayerInputCallback>();

    static PlayerActionManager()
    {
        MultiplayerHooking.OnPlayerAction += OnPlayerAction;
    }

    private static void OnPlayerAction(PlayerID playerId, PlayerActionType type, PlayerID otherPlayer)
    {
        foreach (var tag in PlayerActionTags.GetAll(playerId))
        {
            tag.OnAction(type, otherPlayer);
        }
    }

    private static void CheckHand(Hand hand)
    {
        var controller = hand._controller;
        var handedness = controller.handedness;

        if (controller._menuTap)
            InvokeInputEvent(PlayerInputType.Ability, true, handedness);

        var state = controller.isBelowGripThreshold;
        var lastState = LastGripStateMap.GetValueOrDefault(handedness, false);
        if (state == lastState) return;

        InvokeInputEvent(PlayerInputType.Grip, state, handedness);
        LastGripStateMap[handedness] = controller.isBelowGripThreshold;
    }

    private static void CheckHands()
    {
        if (!NetworkInfo.HasServer)
            return;
        if (!BoneLib.Player.HandsExist)
            return;

        CheckHand(BoneLib.Player.LeftHand);
        CheckHand(BoneLib.Player.RightHand);
    }

    public static void Update()
    {
        CheckHands();
    }

    private static void InvokeInputEvent(PlayerInputType type, bool state, Handedness handedness = Handedness.BOTH)
    {
        if (!NetworkInfo.HasServer)
            return;
        
        foreach (var tag in PlayerInputTags.GetAll(PlayerIDManager.LocalSmallID))
        {
            tag.OnInput(type, state, handedness);
        }
    }
}