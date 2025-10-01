using Il2CppSLZ.Marrow.Audio;
using LabFusion.Extensions;
using MashGamemodeLibrary.Audio.Players.Background.Music;
using MashGamemodeLibrary.Context;
using MelonLoader;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

namespace MashGamemodeLibrary.Audio.Players.Background;

class MusicStateComparer<T> : IComparer<MusicState<T>>
{
    public int Compare(MusicState<T>? x, MusicState<T>? y)
    {
        var xPriority = x?.Priority ?? 0;
        var yPriority = y?.Priority ?? 0;
        
        return xPriority.CompareTo(yPriority);
    }
}

public class MusicPlayer<T, U> where T : GameContext
{
    private readonly SortedSet<MusicState<U>> _states;
    private bool _isActive = false;
    private int _trackIndex = 0;
    private MusicState<U>? _activeState = null;
    private Func<T, U> _contextBuilder;
    private U _context = default!;

    public MusicPlayer(MusicState<U>[] states, Func<T, U> contextBuilder)
    {
        if (states.Length == 0)
            throw new ArgumentException("At least one music state must be provided");
        
        _states = new SortedSet<MusicState<U>>(states, new MusicStateComparer<U>());
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
        var hasOverride = !audio2dManager._overridenMusicClip;

        audio2dManager.StopOverrideMusic();
        if (hasOverride)
            return;

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
    
    private MusicState<U> GetWantedState()
    {
        return _states.FirstOrDefault(musicState => musicState.CanPlay(_context)) ?? _states.Max ?? null!;
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
    }

    public void Update()
    {
        if (!_isActive)
            return;
        
        BuildContext();
        
        var wantedState = GetWantedState();
        if (!IsPlaying() && wantedState == _activeState) return;

        _activeState = wantedState;
        NextTrack();
    }

    private static bool IsPlaying()
    {
        var isOverride = Audio2dPlugin.Audio2dManager._isOverride;
        if (!isOverride) return false;
        
        var currentAmbAndMusic = GetCurrentAmbAndMusic();
        return currentAmbAndMusic == null || !currentAmbAndMusic.ambMus.isPlaying;
    }
    
    private static AmbAndMusic? GetCurrentAmbAndMusic()
    {
        var audio2dManager = Audio2dPlugin.Audio2dManager;
        var curMus = audio2dManager._curMus;
        return curMus > 0 && curMus < audio2dManager.ambAndMusics.Length ? audio2dManager.ambAndMusics[curMus] : null;
    }
}