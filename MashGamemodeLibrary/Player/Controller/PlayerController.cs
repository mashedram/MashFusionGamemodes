using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.Senders;
using MashGamemodeLibrary.Phase;

namespace MashGamemodeLibrary.Player.Controller;

public abstract class PlayerController
{
    public NetworkPlayer Owner { get; private set; } = null!;

    public void Attach(NetworkPlayer player)
    {
        Owner = player;
        OnAttach();
    }

    public void Detach()
    {
        OnDetach();
    }
    
    // Generics

    public virtual void OnAttach()
    {
        
    }

    public virtual void OnDetach()
    {
        
    }
    
    public virtual void OnUpdate(float delta)
    {
        
    }

    public virtual void OnPlayerAction(PlayerActionType action, PlayerID otherPlayer)
    {
        
    }
}