using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace BoneStrike.Tags;

public class IsImportantTag : IComponent, IComponentReady
{
    public void OnReady(NetworkEntity networkEntity, MarrowEntity marrowEntity)
    {
        marrowEntity.PreventDisableOnCull(true);
    }
}