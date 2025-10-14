using UnityEngine;

namespace MashGamemodeLibrary.Vision;

internal class RenderSet
{
    private bool _isValid;
    private bool _hidden;
    private readonly HashSet<GameObject> _gameObjects = new();
    private readonly HashSet<Renderer> _renderers = new();

    public bool IsValid => CheckValidity();

    public RenderSet(GameObject? root, bool hidden)
    {
        _isValid = true;
        _hidden = hidden;
        
        Set(root);
    }

    public RenderSet(bool hidden = false)
    {
        _isValid = false;
        _hidden = hidden;
    }

    private bool CheckValidity()
    {
        if (!_isValid)
            return false;

        foreach (var gameObject in _gameObjects)
        {
            if (gameObject == null)
                _isValid = false;
        }

        return _isValid;
    }
    
    public void Set(GameObject? root, bool? hidden = null)
    {
        if (hidden.HasValue)
        {
            _hidden = hidden.Value;
        }
        
        _renderers.Clear();
        
        if (root == null) return;
        _isValid = true;
        _gameObjects.Clear();
        _gameObjects.Add(root);
        
        var renderers = root.GetComponentsInChildren<Renderer>();
        _renderers.EnsureCapacity(renderers.Count);
        
        foreach (var renderer in renderers)
        {
            if (!renderer)
            {
                continue;
            }
            _renderers.Add(renderer);
            renderer.enabled = !_hidden;
        }
    }

    public void Add(GameObject? root)
    {
        if (!_isValid)
        {
            _renderers.Clear();
            _gameObjects.Clear();
        }

        if (root == null) return;
        _isValid = true;
        _gameObjects.Add(root);
        
        var renderers = root.GetComponentsInChildren<Renderer>();
        _renderers.EnsureCapacity(_renderers.Count + renderers.Count);
        foreach (var renderer in renderers)
        {
            if (!renderer)
                continue;
            _renderers.Add(renderer);
            renderer.enabled = !_hidden;
        }
    }

    public void Clear()
    {
        _renderers.Clear();
        _isValid = false;
    }

    public void SetHidden(bool hidden)
    {
        _hidden = hidden;

        if (!_isValid)
            return;
        
        foreach (var renderer in _renderers)
        {
            if (!renderer)
            {
                _isValid = false;
                return;
            }
            renderer.enabled = !_hidden;
        }
    }
}