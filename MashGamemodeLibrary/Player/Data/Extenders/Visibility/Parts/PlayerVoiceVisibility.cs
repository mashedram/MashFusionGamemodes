using System.Diagnostics.CodeAnalysis;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using UnityEngine;
using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace MashGamemodeLibrary.Player.Spectating.Data.Components.Visibility.Parts;

public class PlayerVoiceVisibility : IPlayerVisibility
{
    private NetworkPlayer? _player;
    
    private bool TryGetAudioSource([MaybeNullWhen(false)] out AudioSource audioSource)
    {
        if (_player == null)
        {
            audioSource = null;
            return false;
        }
        
        audioSource = _player.VoiceSource?.VoiceSource.AudioSource;
        return audioSource != null;
    }
    
    public void SetVisible(bool isVisible)
    {
        if (!TryGetAudioSource(out var audioSource))
            return;

        audioSource.mute = !isVisible; 
    }
    public void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager)
    {
        _player = networkPlayer;
        if (!TryGetAudioSource(out var audioSource))
            return;
    
        // If the avatar changed, we want to make sure the voice is still in the correct state
        audioSource.mute = !audioSource.mute;
    }
    
    public void OnAvatarChanged(Avatar avatar)
    {
        // No-Op
    }
}