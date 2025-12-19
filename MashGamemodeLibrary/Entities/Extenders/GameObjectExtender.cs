using UnityEngine;
using Object = UnityEngine.Object;

namespace MashGamemodeLibrary.Entities.Extenders;

public static class GameObjectExtender
{
    private static readonly HashSet<GameObject> GameObjects = new();

    internal static void DestroyAll()
    {
        foreach (var gameObject in GameObjects) Object.Destroy(gameObject);
        GameObjects.Clear();
    }

    public static GameObject CreateSafeObject(this Transform parent, string name)
    {
        var gameObject = new GameObject
        {
            name = name,
            transform =
            {
                parent = parent,
                localPosition = Vector3.zero,
                localRotation = Quaternion.identity
            }
        };

        GameObjects.Add(gameObject);
        return gameObject;
    }

    public static GameObject CreateSafeObject(this GameObject parent, string name)
    {
        return CreateSafeObject(parent.transform, name);
    }
}