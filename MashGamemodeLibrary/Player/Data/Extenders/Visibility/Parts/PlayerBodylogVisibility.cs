using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Visibility;
using UnityEngine;
using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace MashGamemodeLibrary.Player.Data.Extenders.Visibility.Parts;

public class PlayerBodylogVisibility : IPlayerVisibility
{
    private static readonly string[] BodylogParts = new[]
    {
        "BodyLog/BodyLog",
        "BodyLog/VFX",
        "BodyLog/spheregrip/Sphere/Art/GrabGizmo",
        "BodyLog/PreviewPoint",
        "BodyLog/Dial"
    };

    private bool _isVisible = true;
    private List<GameObject> _bodylogObjects = new();

    public void SetVisible(bool isVisible)
    {
        _isVisible = isVisible;
        foreach (var bodylogObject in _bodylogObjects)
        {
            if (bodylogObject != null)
                bodylogObject.SetActive(isVisible);
        }
    }

    public void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager)
    {
        foreach (var bodylogObject in _bodylogObjects)
        {
            if (bodylogObject != null)
                bodylogObject.SetActive(true);
        }

        _bodylogObjects.Clear();

        var slot = rigManager.inventory.specialItems[0];
        if (slot == null)
            return;

        foreach (var bodylogPart in BodylogParts)
        {
            var bodylogObject = slot.transform.Find(bodylogPart)?.gameObject;
            if (bodylogObject == null) continue;

            _bodylogObjects.Add(bodylogObject);
        }
        SetVisible(_isVisible);
    }

    public void OnAvatarChanged(Avatar avatar)
    {
        // No-Op
    }
}