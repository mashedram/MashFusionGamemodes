using Clockhunt.Phase;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Extenders;
using MashGamemodeLibrary.Entities.Tagging.Base;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Phase.Tags;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Clockhunt.Entities.Tags;

public class ClockLight : EntityTag, IPhaseChangedTag
{
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

        var marrow = Entity.GetExtender<IMarrowEntityExtender>()?.MarrowEntity;
        if (marrow == null) return;

        _lightGo = marrow.gameObject.CreateSafeObject("ClockLight");
        
        var light = _lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = Color.red;
        light.intensity = 1f;
    }
}