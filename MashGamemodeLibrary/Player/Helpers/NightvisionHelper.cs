using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MashGamemodeLibrary.Player.Helpers;

internal struct NightVisionObject
{
    public GameObject GameObject;
    public ColorAdjustments ColorAdjustments;

    public readonly void SetActive(bool state)
    {
        if (!GameObject)
            return;
        GameObject.SetActive(state);
    }

    public readonly void SetColor(Color color)
    {
        ColorAdjustments.colorFilter.value = color;
    }

    public readonly void SetBrightness(float value)
    {
        ColorAdjustments.postExposure.value = value;
    }
}

public static class NightVisionHelper
{
    private static bool _nightVisionEnabled;
    private static float _nightVisionBrightness = 1f;
    private static NightVisionObject? _instance;

    public static bool Enabled
    {
        get => _nightVisionEnabled;
        set => ToggleNightVision(value);
    }

    public static float Brightness
    {
        get => _nightVisionBrightness;
        set {
            _nightVisionBrightness = value;
            _instance?.SetBrightness(value);
        }
    }

    private static void ToggleNightVision(bool isEnabled)
    {
        _nightVisionEnabled = isEnabled;
        
        var go = GetOrCreate();
        go.SetActive(isEnabled);
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
        colorAdjustments.postExposure.value = _nightVisionBrightness; // Slightly increase exposure
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
}