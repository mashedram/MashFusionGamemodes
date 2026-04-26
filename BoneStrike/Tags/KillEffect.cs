using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.Senders;
using MashGamemodeLibrary.Entities.ECS.Attributes;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Execution;

namespace BoneStrike.Tags;

/// <summary>
/// Local only component for the host to detect and broadcast death audio
/// </summary>
[LocalOnly]
public class KillEffect : IComponent, IPlayerAttached, IPlayerActionCallback
{
    private NetworkPlayer? _owner;
    public void OnReady(NetworkPlayer networkPlayer)
    {
        _owner = networkPlayer;
    }

    public void OnAction(PlayerActionType action, PlayerID otherPlayer)
    {
        Executor.RunIfHost(() =>
        {
            if (_owner == null)
                return;
            if (action != PlayerActionType.DYING)
                return;

            if (!_owner.HasRig)
                return;

            var position = _owner.RigRefs.Head.position;
            BoneStrike.Context.KillAudioPlayer.PlayRandom(position);
        });
    }
}