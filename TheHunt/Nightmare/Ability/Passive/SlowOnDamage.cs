using LabFusion.Entities;

namespace TheHunt.Nightmare.Ability.Passive;

public class SlowOnDamage : IUpdatingAbility
{
    private float _maxSpeed;
    private NetworkPlayer? _networkPlayer;
    
    public SlowOnDamage(float maxSpeed)
    {
        _maxSpeed = maxSpeed;
    }
    public void OnAdded(NetworkPlayer networkPlayer)
    {
        _networkPlayer = networkPlayer;
    }
    public void OnRemoved(NetworkPlayer networkPlayer)
    {
        _networkPlayer = null;
    }
    public void Update(float delta)
    {
        if (_networkPlayer is not { HasRig: true })
            return;
    }
}