using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Phase;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IPhaseChanged : IBehaviour
{
    void OnPhaseChange(GamePhase gamePhase); 
}