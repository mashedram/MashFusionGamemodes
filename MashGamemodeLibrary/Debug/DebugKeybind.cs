using UnityEngine;

#if DEBUG
namespace MashGamemodeLibrary.Debug;

public abstract class DebugKeybind
{
    private static bool _isRegistered;
    private static readonly List<DebugKeybind> _keybinds = new();
    private bool _lastKeyState;
    protected abstract KeyCode _key { get; }
    protected abstract Action _onPress { get; }

    private static void Update(DebugKeybind keybind)
    {
        var keyState = Input.GetKey(keybind._key);
        if (keyState != keybind._lastKeyState && keyState)
            keybind._onPress.Invoke();
        keybind._lastKeyState = keyState;
    }

    private static void Register()
    {
        if (_isRegistered) return;
        _isRegistered = true;

        typeof(DebugKeybind).Assembly.DefinedTypes.Where(t => t.IsSubclassOf(typeof(DebugKeybind)) && !t.IsAbstract)
            .ToList().ForEach(t =>
            {
                var instance = (DebugKeybind)Activator.CreateInstance(t)!;
                _keybinds.Add(instance);
            });
    }

    public static void UpdateAll()
    {
        if (!_isRegistered) Register();
        _keybinds.ForEach(Update);
    }
}
#endif