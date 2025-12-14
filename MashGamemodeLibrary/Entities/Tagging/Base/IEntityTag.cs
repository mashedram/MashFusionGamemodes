namespace MashGamemodeLibrary.Entities.Tagging.Base;

public interface IEntityTag
{
    EntityTagIndex GetIndex();
    double CreatedAt();
    bool HasLoaded();
}