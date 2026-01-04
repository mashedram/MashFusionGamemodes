using Clockhunt.Phase;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.Extenders;
using MashGamemodeLibrary.Phase;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Clockhunt.Entities.Tags;

public class ClockLight : IComponent, IComponentReady, IPhaseChanged
{
    private MarrowEntity? _marrowEntity;
    private GameObject? _lightGo;

    public void OnPhaseChange(GamePhase gamePhase)
    {
        if (gamePhase is not HuntPhase)
        {
            if (_lightGo == null) return;

            Object.Destroy(_lightGo);
            _lightGo = null;
            return;
        }
        
        if (_lightGo != null) return;

        if (_marrowEntity == null) return;

        _lightGo = _marrowEntity.gameObject.CreateSafeObject("ClockLight");
        
        var light = _lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = Color.red;
        light.intensity = 1f;
    }
    
    public void OnReady(NetworkEntity networkEntity, MarrowEntity marrowEntity)
    {
        _marrowEntity = marrowEntity;
    }
}