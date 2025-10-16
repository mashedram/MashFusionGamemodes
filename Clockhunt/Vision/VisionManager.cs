using Clockhunt.Config;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using LightType = UnityEngine.LightType;

namespace Clockhunt.Vision;

internal struct NightVisionObject
{
    public GameObject GameObject;
    public ColorAdjustments ColorAdjustments;

    public void SetActive(bool state)
    {
        if (!GameObject)
            return;
        GameObject.SetActive(state);
    }

    public void SetColor(Color color)
    {
        ColorAdjustments.colorFilter.value = color;
    }

    public void SetBrightness(float value)
    {
        ColorAdjustments.postExposure.value = value;
    }
}

public static class VisionManager
{
    private static bool _nightVisionEnabled = false;
    private static NightVisionObject? _instance;

    static VisionManager()
    {
        Clockhunt.OnConfigChanged += config =>
        {
            _instance?.SetActive(_nightVisionEnabled && config.NightVision);
            _instance?.SetBrightness(config.NightVisionBrightness);
        };
    }

    private static NightVisionObject GetOrCreate()
    {
        if (_instance != null && _instance.Value.GameObject) return _instance.Value;

        var go = new GameObject("VisionManager")
        {
            transform =
            {
                rotation = Quaternion.LookRotation(Vector3.down)
            }
        };

        var volume = go.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10;
        volume.weight = 1f;

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        volume.sharedProfile = profile;

        var colorAdjustments = profile.Add<ColorAdjustments>(true);
        colorAdjustments.contrast.value = 20f; // Increase contrast
        colorAdjustments.postExposure.value = Clockhunt.Config.NightVisionBrightness; // Slightly increase exposure
        colorAdjustments.colorFilter.value = Color.white;

        var light = go.AddComponent<Light>();
        light.color = Color.white;
        light.range = 120f;
        light.intensity = 1f;
        light.type = LightType.Directional;
        light.shadows = LightShadows.None;

        _instance = new NightVisionObject
        {
            GameObject = go,
            ColorAdjustments = colorAdjustments
        };
        return _instance.Value;
    }

    public static void EnableNightVision()
    {
        _nightVisionEnabled = true;
        
        if (!Clockhunt.Config.NightVision) return;

        var go = GetOrCreate();
        go.SetActive(true);
    }

    public static void DisableNightVision()
    {
        _nightVisionEnabled = false;
        
        _instance?.SetActive(false);
    }
}