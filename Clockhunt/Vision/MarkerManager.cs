using Il2CppSLZ.Marrow.Pool;
using LabFusion.Marrow.Pool;
using UnityEngine;

namespace Clockhunt.Vision;

public static class MarkerManager
{
    private const string MarkerBarcode = "Sylvie.SignalisMonodiscs.Spawnable.Beam";
    private static Poolee? _marker;
    
    public static void SetMarker(Vector3 position)
    {
        if (_marker)
        {
            _marker!.gameObject.transform.position = position;
            return;
        }

        var spawnable = LocalAssetSpawner.CreateSpawnable(MarkerBarcode);
        LocalAssetSpawner.Register(spawnable);
        
        LocalAssetSpawner.Spawn(spawnable, position, Quaternion.identity, poolee =>
        {
            _marker = poolee;
        });
    }

    public static void ClearMarker()
    {
        if (!_marker) return;
        
        _marker.Despawn();
        _marker = null;
    }
}