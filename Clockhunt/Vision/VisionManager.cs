using Clockhunt.Config;
using MashGamemodeLibrary.networking.Variable.Impl;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using LightType = UnityEngine.LightType;

namespace Clockhunt.Vision;

struct NightVisionObject
{
    public GameObject GameObject;    
    public ColorAdjustments ColorAdjustments;

    public void SetActive(bool state)
    {
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
    private static NightVisionObject? _instance;

    static VisionManager()
    {
        ClockhuntConfig.NightVisionBrightness.OnValueChanged += value =>
        {
            _instance?.SetBrightness(value);
        };

        ClockhuntConfig.NightVisionColor.OnValueChanged += _ =>
        {
            if (!_instance.HasValue) return;
            SetColor(_instance.Value.ColorAdjustments.colorFilter.value);
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
        colorAdjustments.postExposure.value = ClockhuntConfig.NightVisionBrightness; // Slightly increase exposure
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
            ColorAdjustments = colorAdjustments,
        };
        return _instance.Value;
    }

    public static void EnableNightVision()
    {
        if (!ClockhuntConfig.NightVision.Value) return;
        
        var go = GetOrCreate();
        go.SetActive(true);
    }

    public static void SetColor(Color color)
    {
        _instance?.SetBrightness(ClockhuntConfig.NightVisionBrightness);
        
        if (!ClockhuntConfig.NightVisionColor.Value)
        {
            _instance?.SetColor(Color.white);
            return;
        }

        _instance?.SetColor(color);
    }
    
    public static void DisableNightVision()
    {
        _instance?.SetActive(false);
    }
}