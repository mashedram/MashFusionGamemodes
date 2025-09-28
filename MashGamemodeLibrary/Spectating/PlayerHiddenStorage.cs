using Il2CppSLZ.Marrow;
using MelonLoader;
using UnityEngine;

namespace MashGamemodeLibrary.Spectating;

public class PlayerHiddenStorage
{
    public List<MeshRenderer> MeshRenderers = new List<MeshRenderer>();
    public List<SkinnedMeshRenderer> SkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
    public List<Collider> Colliders = new List<Collider>();

    public void Populate(RigManager rigManager)
    {
        MelonLogger.Msg("Populating rigmanager contents to hide...");
        foreach (var meshRenderersEnabled in rigManager.gameObject.GetComponentsInChildren<MeshRenderer>())
        {
            if (!meshRenderersEnabled.enabled) continue;
            
            MelonLogger.Msg("Mesh Renderer Found and disabled. " + meshRenderersEnabled.name);
            MeshRenderers.Add(meshRenderersEnabled);
            meshRenderersEnabled.enabled = false;
        }

        foreach (var skinnedMeshRendererEnabled in rigManager.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (!skinnedMeshRendererEnabled.enabled) continue;
            
            MelonLogger.Msg("Mesh Renderer Found and disabled. " + skinnedMeshRendererEnabled.name);
            SkinnedMeshRenderers.Add(skinnedMeshRendererEnabled);
            skinnedMeshRendererEnabled.enabled = false;
        }

        /*foreach (var colliderEnabled in rigManager.gameObject.GetComponentsInChildren<Collider>())
        {
            if (colliderEnabled.enabled)
            {
                Colliders.Add(colliderEnabled);
                colliderEnabled.enabled = false;
            }
        }*/
    }

    public void Show()
    {
        MelonLogger.Msg("Showing rigmanager contents...");
        foreach (var meshRenderer in MeshRenderers)
        {
            MelonLogger.Msg("Mesh Renderer Found and enabled. " + meshRenderer.name);
            meshRenderer.enabled = true;
        }

        foreach (var skinnedMeshRenderer in SkinnedMeshRenderers)
        {
            MelonLogger.Msg("Skinned Renderer Found and enabled. " + skinnedMeshRenderer.name);
            skinnedMeshRenderer.enabled = true;
        }

        /*foreach (var collider in Colliders)
        {
            collider.enabled = true;
        }*/
    }
}