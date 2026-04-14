using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Visibility;
using UnityEngine;
using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace MashGamemodeLibrary.Player.Data.Extenders.Visibility.Parts;

public class PlayerWindbuffetVisibility : IPlayerVisibility
{
    private bool _isVisible = true;
    private GameObject? _holder;
    
    public void SetVisible(bool isVisible)
    {
        _isVisible = isVisible;
        if (_holder == null)
            return;
        _holder.SetActive(_isVisible);
    }
    
    public void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager)
    {
        var transform = rigManager.transform.Find("VRControllerRig/TrackingSpace/Headset/WindBuffetSFX");
        if (transform == null)
            return;
        _holder = transform.gameObject;
        
        if (_holder == null)
            return;
        _holder.SetActive(_isVisible);
    }
    
    public void OnAvatarChanged(Avatar avatar)
    {
        // NO-OP
    }
}