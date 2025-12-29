using Il2CppSLZ.Marrow;
using MashGamemodeLibrary.Player.Collision;
using MashGamemodeLibrary.Player.Visibility;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MashGamemodeLibrary.Debug;

public class DebugSelfKeybind : DebugKeybind
{

    protected override KeyCode _key => KeyCode.L;
    protected override Action _onPress { get; } = () =>
    {
        var rig = Object.FindObjectOfType<PhysicsRig>();
        foreach (var collider in rig.GetComponentsInChildren<Collider>().Where(c => !PlayerColliderManager.IsLocalDisabled(c) && c.gameObject.active))
        {
            var gameObject = collider.gameObject;
            MelonLogger.Msg($"Child: {gameObject.name} with: {gameObject.layer}");
        }
    };
}