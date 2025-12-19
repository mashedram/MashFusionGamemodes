using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;

namespace MashGamemodeLibrary.Entities.Tagging.Base;

public interface IMarrowLoaded
{
     void OnLoaded(NetworkEntity networkEntity, MarrowEntity marrowEntity);
}