using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.ECS.Attributes;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using Object = UnityEngine.Object;

namespace MashGamemodeLibrary.Entities.CommonComponents;

public class ForcePull : IEntityAttached, IRemoved
{
    private InteractableHost? _interactableHost;
    private ForcePullGrip? _forcePullGrip;

    public void OnReady(NetworkEntity networkEntity, MarrowEntity marrowEntity)
    {
        _interactableHost = marrowEntity._behaviours
            .OfType<InteractableHost>()
            .FirstOrDefault();
        
        if (_interactableHost == null)
            return;

        var grip = _interactableHost
            .GetGrip();
        
        if (grip == null)
            return;

        var fp = grip.gameObject.AddComponent<ForcePullGrip>();
        _interactableHost._fpGrips.Add(fp);
        _forcePullGrip = fp;
    }
    
    public void OnRemoved()
    {
        if (_forcePullGrip == null)
            return;
        
        if (_interactableHost != null)
            _interactableHost._fpGrips.Remove(_forcePullGrip);
        
        Object.Destroy(_forcePullGrip);
    }
}