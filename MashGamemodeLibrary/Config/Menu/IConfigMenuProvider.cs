using LabFusion.Menu.Data;

namespace MashGamemodeLibrary.Config.Menu;

public interface IConfigMenuProvider
{
    void AddExtraFields(GroupElementData root);
}