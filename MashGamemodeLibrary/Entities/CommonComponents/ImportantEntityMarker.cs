using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace MashGamemodeLibrary.Entities.CommonComponents;

public class ImportantEntityMarker : IComponent, IEntityAttached
{
    public void OnReady(NetworkEntity networkEntity, MarrowEntity marrowEntity)
    {
        marrowEntity.PreventDisableOnCull(true);
    }
}