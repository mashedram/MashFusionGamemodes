using System.Diagnostics.CodeAnalysis;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Spectating.Data.Components.Visibility.Parts;

public class PlayerVoiceVisibility : IPlayerVisibility
{
    private NetworkPlayer _player;
    
    public PlayerVoiceVisibility(NetworkPlayer player)
    {
        _player = player;
    }
    
    private bool TryGetAudioSource([MaybeNullWhen(false)] out AudioSource audioSource)
    {
        audioSource = _player.VoiceSource?.VoiceSource.AudioSource;
        return audioSource != null;
    }
    
    public void SetVisible(bool isVisible)
    {
        if (!TryGetAudioSource(out var audioSource))
            return;

        audioSource.mute = !isVisible; 
    }
    
    public void OnRigChanged(RigManager? rigManager)
    {
        if (!TryGetAudioSource(out var audioSource))
            return;
    
        // If the avatar changed, we want to make sure the voice is still in the correct state
        audioSource.mute = !audioSource.mute;
    }
}