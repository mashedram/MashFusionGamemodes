using LabFusion.Player;
using MashGamemodeLibrary.Player.Spawning;
using UnityEngine;

namespace MashGamemodeLibrary.Debug;

public class ToSpawnPointKeybind : DebugKeybind
{

    protected override KeyCode _key => KeyCode.U;
    protected override Action _onPress { get; } = () =>
    {
        const float range = 60f;
        var start = BoneLib.Player.Head.position;
        DynamicSpawnCollector.CollectAt(start, range);
        var target = DynamicSpawnCollector.GetRandomPoint(30, start, new AvoidSpawningNear(start, range/4f));
        if (target != null)
            LocalPlayer.TeleportToPosition(target.Value);
    };
}