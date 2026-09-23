using LabFusion.Entities;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.Extenders;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheHunt.Nightmare.Ability.Passive;

public class FaintGlowAbility : IAbility, IPlayerAttached, IRemoved
{
    private GameObject? _light;
    
    public void OnReady(NetworkPlayer networkPlayer)
    {
        _light = networkPlayer.RigRefs.Head.CreateSafeObject("EntityLight");
        var light = _light.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = Color.red;
        light.range = 90f;
        light.spotAngle = 90f;
        light.intensity = 5f;

        light.shadows = LightShadows.Soft;

        _light.transform.localPosition = Vector3.forward * 0.5f;
        _light.transform.localRotation = Quaternion.identity;
    }
    
    public void OnRemoved()
    {
        if (_light == null)
            return;
        
        Object.Destroy(_light);
        _light = null;
    }
}