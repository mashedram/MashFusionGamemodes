using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;

namespace TheHunt.Nightmare.Ability;

public interface IActiveAbility : IAbility
{
    Handedness Handedness { get; }
    /// <summary>
    /// ONLY CALLED LOCALLY ON THE INPUT SENDER
    /// </summary>
    /// <param name="networkPlayer"></param>
    void UseAbility(NetworkPlayer networkPlayer);
    float Cooldown { get; }
}