using UnityEngine;

namespace MashGamemodeLibrary.Audio.Modifiers;

public class AudioSettingsModifier : IAudioModifier
{
    private float _volume = 1.0f;
    private float _maxDistance = 120.0f;
    private float _spatialBlend = 1.0f;
    private bool _loop = false;
    private AnimationCurve? _customRolloff;
    
    public AudioSettingsModifier SetVolume(float volume)
    {
        _volume = volume;
        return this;
    }
    
    public AudioSettingsModifier SetMaxDistance(float maxDistance)
    {
        _maxDistance = maxDistance;
        return this;
    }
    
    public AudioSettingsModifier SetSpatialBlend(float spatialBlend)
    {
        _spatialBlend = spatialBlend;
        return this;
    }
    
    public AudioSettingsModifier SetLoop(bool loop)
    {
        _loop = loop;
        return this;
    }
    
    public AudioSettingsModifier SetCustomRolloff(AnimationCurve curve)
    {
        _customRolloff = curve;
        return this;
    }
    
    public void OnStart(ref AudioSource source)
    {
        source.volume = _volume;
        source.maxDistance = _maxDistance;
        source.spatialBlend = _spatialBlend;
        source.loop = _loop;
        if (_customRolloff != null)
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, _customRolloff);
    }
}

