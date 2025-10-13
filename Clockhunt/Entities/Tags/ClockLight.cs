using Clockhunt.Phase;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Tagging.Base;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Phase.Tags;
using UnityEngine;

namespace Clockhunt.Entities.Tags;

public class ClockLight : EntityTag, ITagRemoved, IPhaseChangedTag
{
    private Light? _light;

    public void OnRemoval(ushort entityID)
    {
        if (_light == null) return;
        UnityEngine.Object.Destroy(_light);
        _light = null;
    }

    public void OnPhaseChange(GamePhase gamePhase)
    {
        if (gamePhase is not HuntPhase)
        {
            if (_light == null) return;
            UnityEngine.Object.Destroy(_light);
            _light = null;
            return;
        }

        var marrow = Entity.GetExtender<IMarrowEntityExtender>()?.MarrowEntity;
        if (marrow == null) return;

        _light = marrow.gameObject.GetComponent<Light>() ?? marrow.gameObject.AddComponent<Light>();
        _light.type = LightType.Point;
        _light.color = Color.red;
        _light.intensity = 1f;
    }
}