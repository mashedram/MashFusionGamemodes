using Il2CppSLZ.Marrow.Audio;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Players.Extensions;
using MelonLoader;
using Random = UnityEngine.Random;

namespace MashGamemodeLibrary.Audio.Players.Background;

public class MusicPlayer : IContinuousPlayer
{
    private readonly IAudioContainer _audioContainer;
    private int _trackIndex;

    public MusicPlayer(IAudioContainer audioContainer)
    {
        _audioContainer = audioContainer;
    }

    public void StartPlaying()
    {
        if (IsActive)
            return;
        IsActive = true;

        NextTrack();
    }

    public void StopPlaying()
    {
        if (!IsActive)
            return;
        IsActive = false;

        StopTrack();
    }

    public bool IsActive { get; private set; }

    public void Update(float delta)
    {
        if (!IsActive) return;
        if (IsPlaying()) return;

        NextTrack();
    }

    private void PlayTrack()
    {
        var name = _audioContainer.AudioNames[_trackIndex];
        if (string.IsNullOrEmpty(name)) return;
        _audioContainer.RequestClip(name, clip =>
        {
            if (!clip)
            {
                MelonLogger.Error($"Failed to load music clip: {name}");
                return;
            }

#if DEBUG
            MelonLogger.Msg($"Playing music clip: {name} from effector: {GetType().FullName}");
#endif

            var audio2dManager = Audio2dPlugin.Audio2dManager;
            const float musicVolume = 0.8f;
            audio2dManager.CueOverrideMusic(clip, musicVolume, 2.0f, 2.0f, false, false);
        });
    }

    private void NextTrack()
    {
        var audioNames = _audioContainer.AudioNames;
        if (audioNames.Count == 0)
            return;

        var offset = Random.Range(1, audioNames.Count);
        _trackIndex = (_trackIndex + offset) % audioNames.Count;

        PlayTrack();
    }

    private static void StopTrack()
    {
        var audio2dManager = Audio2dPlugin.Audio2dManager;
        if (!audio2dManager)
            return;
        audio2dManager.StopOverrideMusic();
        audio2dManager.StopMusic(0.2f);
    }

    private bool IsPlaying()
    {
        if (_audioContainer.IsLoading)
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