using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Senders;
using UnityEngine;

namespace Clockhunt.Game;

class PlayerPath
{
    private List<Vector3> _points = new();
    
    public void AddPoint(Vector3 point)
    {
        _points.Add(point);
    }
    
    public Vector3 GetPoint(int index)
    {
        return _points[index];
    }
    
    public int PointCount => _points.Count;
    
    public void Clear()
    {
        _points.Clear();
    }
}

class LocalSpawnCollector
{
    private static readonly LayerMask EnvironmentLayerMask = Physics.DefaultRaycastLayers & ~(1 << 8); // Ignore players
    private const double CollectInterval = 1.0;
    private NetworkPlayer _player;
    private PlayerPath _currentPath = new();
    private double _lastCollectTime;
    
    private float GetAvatarHeight()
    {
        return _player.RigRefs?.RigManager?.avatar?.height ?? 1.6f;
    }
    
    private bool IsPointValid(Vector3 point)
    {
        var rigManager = _player.RigRefs?.RigManager;
        if (!rigManager)
            return false;
        
        if (rigManager!.bodyState != RigManager.BodyState.OnFoot)
            return false;
        
        if (rigManager.physicsRig.footSupported > 0.1f)
            return false;

        return true;
    }
    
    private Vector3? GetFloorPoint(Vector3 point)
    {
        var height = GetAvatarHeight();
        
        var ray = new Ray(point, Vector3.down);
        return Physics.Raycast(ray, out var hitInfo, height, EnvironmentLayerMask) ? hitInfo.point : null;
    }

    private void CollectPath()
    {
        var path = _currentPath;
        var height = GetAvatarHeight();

        var validPoints = new List<Vector3>();
        var centerIndex = path.PointCount / 2;
        for (var i = 0; i < path.PointCount; i++)
        {
            // Move from center of path outwards
            var offsetDir = (i % 2 == 0) ? 1 : -1;
            var offsetAmount = (i + 1) / 2;
            var offsetIndex = centerIndex + (offsetDir * offsetAmount);
            if (offsetIndex < 0 || offsetIndex >= path.PointCount)
                break;
            
            var point = path.GetPoint(offsetIndex);
            
            var center = point + Vector3.up * (height * 0.5f);
            var halfExtents = new Vector3(0.2f, height * 0.5f, 0.2f);
            var overlap = Physics.CheckBox(center, halfExtents);
            
            if (overlap)
                continue;

            validPoints.Add(point);
        }
        
        // TODO: Set as valid spawn point for this path
    }

    public void Update()
    {
        var time = Time.timeSinceLevelLoadAsDouble;
        if (time - _lastCollectTime < CollectInterval)
            return;
        _lastCollectTime = time;
        
        var position = _player.RigRefs?.Head.position;
        if (!position.HasValue)
            return;
        
        var floorPoint = GetFloorPoint(position.Value);
        
        if (!floorPoint.HasValue || !IsPointValid(floorPoint.Value))
        {
            CollectPath();
            return;
        }

        _currentPath.AddPoint(floorPoint.Value);
    }
    
    public void OnLocalPlayerAction(PlayerActionType actionType)
    {
        CollectPath();
    }
}

public class SpawnCollector
{
    
}