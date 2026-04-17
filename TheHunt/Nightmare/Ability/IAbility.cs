using LabFusion.Entities;

namespace TheHunt.Nightmare.Ability;

public interface IAbility
{
    void OnAdded(NetworkPlayer networkPlayer);
    void OnRemoved(NetworkPlayer networkPlayer);
}