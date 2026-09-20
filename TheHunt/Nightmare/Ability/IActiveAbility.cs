using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;

namespace TheHunt.Nightmare.Ability;

public interface IActiveAbility : IAbility
{
    string Description { get; }
    
    Handedness Handedness { get; }
    /// <summary>
    /// ONLY CALLED LOCALLY ON THE INPUT SENDER
    /// </summary>
    void UseAbility(Nightmare nightmare, NetworkPlayer networkPlayer);
    float Cooldown { get; }
}