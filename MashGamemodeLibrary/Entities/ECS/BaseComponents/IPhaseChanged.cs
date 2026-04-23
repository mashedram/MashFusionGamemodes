using MashGamemodeLibrary.Entities.Behaviour.Cache;
using MashGamemodeLibrary.Phase;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IPhaseChanged : IBehaviour
{
    void OnPhaseChange(GamePhase gamePhase);
}