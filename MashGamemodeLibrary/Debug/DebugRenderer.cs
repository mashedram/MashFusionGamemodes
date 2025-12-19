using UnityEngine;
using Object = UnityEngine.Object;

#if DEBUG
namespace MashGamemodeLibrary.Debug;

public class DebugRenderer
{
    private static readonly Texture2D LitmasTexture = CreateColorTexture(new Color(0, 255, 59));
    private static readonly List<GameObject> LineRenderers = new();

    private static Texture2D CreateColorTexture(Color color)
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }

    private static void RenderLine(Vector3[] positions, Color color)
    {
        var go = new GameObject("DebugRay");
        var lineRenderer = go.AddComponent<LineRenderer>();
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;

        var material = new Material(Shader.Find("SLZ/LitMAS/LitMAS Standard"))
        {
            color = color
        };
        material.SetTexture("_MetallicGlossMap", LitmasTexture);
        material.SetFloat("_Emission", 1f);
        material.SetColor("_EmissionColor", color);

        lineRenderer.material = material;

        LineRenderers.Add(go);
    }

    public static void RenderCube(Vector3 center, Vector3 size, Color? color = null)
    {
        var half = size / 2f;

        // 8 corners
        var corners = new Vector3[8];
        corners[0] = center + new Vector3(-half.x, -half.y, -half.z);
        corners[1] = center + new Vector3(half.x, -half.y, -half.z);
        corners[2] = center + new Vector3(half.x, -half.y, half.z);
        corners[3] = center + new Vector3(-half.x, -half.y, half.z);
        corners[4] = center + new Vector3(-half.x, half.y, -half.z);
        corners[5] = center + new Vector3(half.x, half.y, -half.z);
        corners[6] = center + new Vector3(half.x, half.y, half.z);
        corners[7] = center + new Vector3(-half.x, half.y, half.z);

        // 12 edges (24 points, each edge is two points)
        var edgePoints = new[]
        {
            // Bottom face
            corners[0],
            corners[1],
            corners[1],
            corners[2],
            corners[2],
            corners[3],
            corners[3],
            corners[0],
            // Top face
            corners[4],
            corners[5],
            corners[5],
            corners[6],
            corners[6],
            corners[7],
            corners[7],
            corners[4],
            // Vertical edges
            corners[0],
            corners[4],
            corners[1],
            corners[5],
            corners[2],
            corners[6],
            corners[3],
            corners[7]
        };

        RenderLine(edgePoints, color ?? Color.green);
    }

    public static void Clear()
    {
        LineRenderers.ForEach(Object.Destroy);
        LineRenderers.Clear();
    }
}
#endif