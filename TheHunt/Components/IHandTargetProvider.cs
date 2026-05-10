using UnityEngine;

namespace TheHunt.Components;

public interface IHandTargetProvider
{
    Vector3? GetHandTargetPosition();
}