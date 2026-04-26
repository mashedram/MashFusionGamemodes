using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.Entities.Association;

public interface IEcsAssociation : INetSerializable
{
    int GetID();
    void HookReady(Action action);
    void HookRemoval(Action action);
}