using BoneLib;
using Il2CppSLZ.Bonelab;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Util;
using TheHunt.Gamemode;
using UnityEngine;
using Object = System.Object;

namespace TheHunt.Scene;

class LightInstance
{
    private readonly Light _light;
    private bool _state = true;
    private float _lastIntensity;
    
    public LightInstance(Light light)
    {
        _light = light;
    }

    public void SetState(bool state)
    {
        if (_state == state) 
            return;
        _state = state;

        if (state)
        {
            if (_light.intensity > 0.5f)
                return;
        
            _light.intensity = _lastIntensity;
        }
        else
        {
            _lastIntensity = _light.intensity;
            _light.intensity = 0f;
        }
        // The light got turned on by something else, don't override it
        
    }
}

class FlickerEventPacket : INetSerializable
{
    public float Duration = 0.5f;
    
    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Duration);
    }
}

class RenderMaterial
{
    private const string EmissionKeyword = "_EMISSION";
    private const string EmissionColorKeyword = "_EmissionColor";
    private static readonly int Emission = Shader.PropertyToID(EmissionKeyword);
    private static readonly int EmissionColor = Shader.PropertyToID(EmissionColorKeyword);
    private readonly Material _material;
    
    private bool _state = true;
    private float? _lastEmission;
    private Color? _lastEmissionColor;
    
    public RenderMaterial(Material material)
    {
        _material = material;
    }

    public static bool IsValidMaterial(Material material)
    {
        if (!material.HasProperty(Emission))
            return false;
        
        if (material.GetFloat(Emission) <= 0.1f)
            return false;

        return true;
    }
    
    public void SetState(bool state)
    {
        if (_state == state) 
            return;
        _state = state;
        
        if (_lastEmission == null || !state)
            _lastEmission = _material.GetFloat(Emission);
        
        if (_lastEmissionColor == null || !state)
            _lastEmissionColor = _material.GetColor(EmissionColor);
        
        _material.SetFloat(Emission, state ? _lastEmission.Value : 0f);
        _material.SetColor(EmissionColor, state ? _lastEmissionColor.Value : Color.black);
    }
}

[RequireStaticConstructor]
public static class LightManager
{
    private static readonly List<LightInstance> LightInstances = new List<LightInstance>();
    private static readonly List<RenderMaterial> RenderMaterials = new List<RenderMaterial>();
    
    // Ambience
    private static LightProbes? _lightProbes;
    private static float? _ambientIntensity;
    private static Color? _lastAmbientColor;
    
    // Synced State
    private static readonly SyncedVariable<bool> BlackoutEnabled = new SyncedVariable<bool>("TheHunt_BlackoutEnabled", new BoolEncoder(), false, CommonNetworkRoutes.HostToAll);
    
    // Local State Helpers
    
    private static bool _localLightsEnabled;

    public static bool IsBlackout => BlackoutEnabled.Value;
    
    static LightManager()
    {
        Hooking.OnLevelLoaded += OnLevelLoaded;

        BlackoutEnabled.OnValueChanged += OnBlackoutEnabledChanged;
    }
    
    private static void OnLevelLoaded(LevelInfo levelInfo)
    {
        _lastAmbientColor = null;
        _ambientIntensity = null;
        _lightProbes = null;
        
        LightInstances.Clear();
        foreach (var light in UnityEngine.Object.FindObjectsOfType<Light>())
        {
            var go = light.gameObject;
            if (go.isStatic)
                return;
            
            LightInstances.Add(new LightInstance(light));
        }
        
        RenderMaterials.Clear();
        var materials = new HashSet<Material>();
        foreach (var renderer in UnityEngine.Object.FindObjectsOfType<Renderer>())
        {
            var go = renderer.gameObject;
            if (go.isStatic)
                return;
            
            foreach (var material in renderer.sharedMaterials)
            {
                if (material == null)
                    continue;
                
                if (!RenderMaterial.IsValidMaterial(material))
                    continue;
                
                if (materials.Add(material))
                {
                    RenderMaterials.Add(new RenderMaterial(material));
                }
            }
        }
    }
    
    private static void SetLights(bool state)
    {
        foreach (var lightInstance in LightInstances)
        {
            lightInstance.SetState(state);
        }
        
        foreach (var renderMaterial in RenderMaterials)
        {
            renderMaterial.SetState(state);
        }
        
        if (_lastAmbientColor == null || !state)
        {
            _lastAmbientColor = RenderSettings.ambientLight;
        }
        if (_ambientIntensity == null || !state) 
        {
            _ambientIntensity = RenderSettings.ambientIntensity;
        }
        if (_lightProbes == null || !state)
        {
            _lightProbes = LightmapSettings.lightProbes;
        }
        
        RenderSettings.ambientLight = state ? _lastAmbientColor.Value : Color.black;
        RenderSettings.ambientIntensity = state ? _ambientIntensity.Value : 0f;
        DynamicGI.UpdateEnvironment();
        
        LightmapSettings.lightProbes = state ? _lightProbes : null;
        LightmapSettings.lightmapsMode = state ? LightmapsMode.CombinedDirectional : LightmapsMode.NonDirectional;
    }

    public static void SetBlackout(bool state)
    {
        Executor.RunIfHost(() =>
        {
            BlackoutEnabled.Value = state;
        });
    }
    
    // Event Handlers
    
    private static void OnBlackoutEnabledChanged(bool isBlackout)
    {
        // Set blackout data
        if (isBlackout && _localLightsEnabled)
        {
            Gamemode.TheHunt.Context.BlackoutAudioPlayer.PlayRandom();
        }
        _localLightsEnabled = !isBlackout;
        
        // Set the lights on
        SetLights(_localLightsEnabled);
    }
}