using UnityEngine;

namespace MashGamemodeLibrary.Vision;

internal class RenderSet
{
    private readonly HashSet<GameObject> _gameObjects = new();
    private readonly HashSet<Renderer> _renderers = new();
    private bool _hidden;

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

        _renderers.Clear();

        if (root == null) return true;
        
        _gameObjects.Clear();
        _gameObjects.Add(root);

        var renderers = root.GetComponentsInChildren<Renderer>();
        _renderers.EnsureCapacity(renderers.Count);

        foreach (var renderer in renderers)
        {
            if (!renderer) 
                return false;
            
            _renderers.Add(renderer);
            renderer.enabled = !_hidden;
        }

        return true;
    }

    public bool Add(GameObject? root)
    {
        if (root == null) return true;

        _gameObjects.Add(root);

        var renderers = root.GetComponentsInChildren<Renderer>();
        _renderers.EnsureCapacity(_renderers.Count + renderers.Count);
        foreach (var renderer in renderers)
        {
            if (!renderer)
                return false;

            _renderers.Add(renderer);
            renderer.enabled = !_hidden;
        }

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

            renderer.enabled = !_hidden;
        }

        return true;
    }

    public bool AllValid()
    {
        return _renderers.All(r => r != null);
    }
}