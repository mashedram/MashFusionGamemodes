using LabFusion.Extensions;
using UnityEngine;

namespace MashGamemodeLibrary.Vision;

internal class RenderSet
{
    private HashSet<Renderer> _renderers = new(new UnityComparer());
    private readonly Dictionary<Renderer, bool> _originalRendererStates = new(new UnityComparer());
    private bool _hidden;

    private bool ShouldBeEnabled(Renderer renderer, bool force)
    {
        if (force)
            return false;

        return _originalRendererStates.GetValueOrDefault(renderer, true);
    }

    public RenderSet(GameObject? root, bool hidden)
    {
        _hidden = hidden;

        Set(root);
    }

    public RenderSet(bool hidden = false)
    {
        _hidden = hidden;
    }

    public bool Set(GameObject? root, bool? hidden = null)
    {
        if (hidden.HasValue) _hidden = hidden.Value;
        
        if (root == null)
        {
            _renderers.Clear();
            return true;
        }
        
        var renderers = root.GetComponentsInChildren<Renderer>();
        var newRenderers = new HashSet<Renderer>(renderers.Count, new UnityComparer());
        _originalRendererStates.EnsureCapacity(renderers.Count);

        foreach (var renderer in renderers)
        {
            if (!renderer) 
                return false;
            
            newRenderers.Add(renderer);
            _originalRendererStates[renderer] = renderer.enabled;
            _renderers.Remove(renderer);
            if (_hidden)
                renderer.enabled = false;
        }

        foreach (var renderer in _renderers)
        {
            _originalRendererStates.Remove(renderer);
        }
        _renderers = newRenderers;
        
        return true;
    }

    public void Clear()
    {
        _renderers.Clear();
    }

    public bool SetHidden(bool hidden)
    {
        _hidden = hidden;

        if (_renderers.Count == 0)
            return true;
        
        foreach (var renderer in _renderers)
        {
            if (!renderer)
            {
                return false;
            }

            renderer.enabled = ShouldBeEnabled(renderer, _hidden);
        }

        return true;
    }

    public bool AllValid()
    {
        return _renderers.All(r => r != null);
    }
}