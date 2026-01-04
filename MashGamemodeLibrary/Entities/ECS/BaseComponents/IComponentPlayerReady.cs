using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;

namespace MashGamemodeLibrary.Entities.ECS.BaseComponents;

public interface IComponentPlayerReady : IPlayerBehaviour
{
    void OnReady(NetworkPlayer networkPlayer, MarrowEntity marrowEntity);
}