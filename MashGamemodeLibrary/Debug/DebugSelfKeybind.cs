using Il2CppSLZ.Marrow;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MashGamemodeLibrary.Debug;

public class DebugSelfKeybind : DebugKeybind
{

    protected override KeyCode _key => KeyCode.L;
    protected override Action _onPress { get; } = () =>
    {
        var rig = Object.FindObjectOfType<PhysicsRig>();
        foreach (var collider in rig.GetComponentsInChildren<Collider>())
        {
            MelonLoader.MelonLogger.Msg($"Child: {collider.gameObject.name} with: {collider.gameObject.layer}");
        }
    };
}