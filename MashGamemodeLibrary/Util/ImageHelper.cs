using UnityEngine;

namespace MashGamemodeLibrary.Util;

public static class ImageHelper
{
    public static Texture2D? LoadEmbeddedImage<T>(string resourceName)
    {
        var assembly = typeof(T).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) 
        {
            InternalLogger.Debug($"Embedded resource not found: {resourceName}");
            return null;
        }
            
        var buffer = new byte[stream.Length];
        _ = stream.Read(buffer, 0, buffer.Length);
            
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
        {
            name = resourceName,
            hideFlags = HideFlags.DontUnloadUnusedAsset
        };
        
        if (!texture.LoadImage(buffer))
        {
            InternalLogger.Debug($"Failed to load image from resource: {resourceName}");
            return null;
        }
        
        texture.Apply();
        return texture;
    }
}