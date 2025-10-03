using Il2CppSLZ.Marrow.Audio;
using MashGamemodeLibrary.Audio.Players.Background.Music;
using MashGamemodeLibrary.Context;
using MelonLoader;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MashGamemodeLibrary.Audio.Environment;

class EnvironmentStateComparer<T> : IComparer<EnvironmentState<T>>
{
    public int Compare(EnvironmentState<T>? x, EnvironmentState<T>? y)
    {
        var xPriority = x?.Priority ?? 0;
        var yPriority = y?.Priority ?? 0;
        
        return yPriority.CompareTo(xPriority);
    }
}

public class EnvironmentPlayer<T, TCustomContext> where T : GameContext
{
    private readonly SortedSet<EnvironmentState<TCustomContext>> _states;
    private bool _isActive;
    private int _trackIndex;
    private EnvironmentState<TCustomContext>? _activeState;
    private Func<T, TCustomContext> _contextBuilder;
    private TCustomContext _context = default!;

    public EnvironmentPlayer(EnvironmentState<TCustomContext>[] states, Func<T, TCustomContext> contextBuilder)
    {
        if (states.Length == 0)
            throw new ArgumentException("At least one music state must be provided");
        
        _states = new SortedSet<EnvironmentState<TCustomContext>>(states, new EnvironmentStateComparer<TCustomContext>());
        _contextBuilder = contextBuilder;
    }

    private void BuildContext()
    {
        var context = GamemodeWithContext<T>.Context;
        _context = _contextBuilder(context);
    }

    private void PlayTrack()
    {
        var state = _activeState;
        if (state == null) return;

        var name = state.GetAudioContainer().AudioNames[_trackIndex];
        if (string.IsNullOrEmpty(name)) return;
        state.GetAudioContainer().RequestClip(name, clip =>
        {
            if (!clip)
            {
                MelonLogger.Error($"Failed to load music clip: {name}");
                return;
            }
            
            #if DEBUG
            MelonLogger.Msg($"Playing music clip: {name} from state: {state.GetType().FullName}");
            #endif
            
            var audio2dManager = Audio2dPlugin.Audio2dManager;
            const float musicVolume = 0.8f;
            audio2dManager.CueOverrideMusic(clip, musicVolume, 2.0f, 2.0f, false, false);
        });
    }

    private static void StopTrack()
    {
        var audio2dManager = Audio2dPlugin.Audio2dManager;
        audio2dManager.StopOverrideMusic();
        audio2dManager.StopMusic(0.2f);
    }
    
    private void NextTrack()
    {
        if (!_isActive)
            return;
        if (_activeState == null)
            return;

        var audioNames = _activeState.GetAudioContainer().AudioNames;
        if (audioNames.Count == 0)
            return;

        var offset = Random.Range(1, audioNames.Count);
        _trackIndex = (_trackIndex + offset) % audioNames.Count;
        
        PlayTrack();
    }
    
    private EnvironmentState<TCustomContext> GetWantedState()
    {
        return _states.FirstOrDefault(musicState => musicState.CanPlay(_context)) ?? _states.Max ?? null!;
    }

    private void UpdateWeather()
    {
        if (_activeState == null)
            return;
        
        if (!_activeState.ShouldApplyWeatherEffects(_context))
            return;

        var weatherEffects = _activeState.GetWeatherSpawnables();
        WeatherManager.SetWeather(weatherEffects);
    }

    public void StartPlaying()
    {
        if (_isActive)
            return;
        
        _isActive = true;
    }
    
    public void StopPlaying()
    {
        if (!_isActive)
            return;
        
        _isActive = false;
        _activeState = null;
        StopTrack();
        WeatherManager.SetWeather(Array.Empty<string>());
    }

    public void Update()
    {
        if (!_isActive)
            return;
        
        BuildContext();
        
        var wantedState = GetWantedState();
        if (IsPlaying() && wantedState == _activeState) return;

        _activeState = wantedState;
        UpdateWeather();
        NextTrack();
    }

    private bool IsPlaying()
    {
        if (_activeState != null && _activeState.GetAudioContainer().IsLoading)
            return true;
        
        var isOverride = Audio2dPlugin.Audio2dManager._isOverride;
        if (!isOverride) return false;
        
        var currentAmbAndMusic = GetCurrentAmbAndMusic();
        return currentAmbAndMusic != null && currentAmbAndMusic.ambMus.isPlaying;
    }
    
    private static AmbAndMusic? GetCurrentAmbAndMusic()
    {
        var audio2dManager = Audio2dPlugin.Audio2dManager;
        var curMus = audio2dManager._curMus;
        return curMus > 0 && curMus < audio2dManager.ambAndMusics.Length ? audio2dManager.ambAndMusics[curMus] : null;
    }
}