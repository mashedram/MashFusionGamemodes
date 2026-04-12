using Il2CppSLZ.Marrow;
using MashGamemodeLibrary.Player.Spectating.data.Rules;

namespace MashGamemodeLibrary.Player.Spectating.data.Components;

public interface IPlayerExtender
{
    void OnRigChanged(RigManager? rigManager);
    void OnRuleChanged(IPlayerRule rule);
}