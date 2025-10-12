using MashGamemodeLibrary.Entities.Tagging.Base;

namespace MashGamemodeLibrary.Phase.Tags;

public interface IPhaseChangedTag : IAbstractEntityTag
{
    void OnPhaseChange(GamePhase gamePhase);
}