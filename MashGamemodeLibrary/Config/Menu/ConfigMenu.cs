using System.Reflection;
using LabFusion.Menu.Data;

namespace MashGamemodeLibrary.Config.Menu;

public class ConfigMenu
{
    private readonly IConfig _instance;
    private readonly List<ConfigMenuField> _fields = new();

    public ConfigMenu(IConfig instance)
    {
        _instance = instance;

        var configType = instance.GetType();
        var fields = configType.GetFields();
        
        foreach (var field in fields)
        {
            var entry = field.GetCustomAttribute<ConfigMenuEntry>();
            if (entry == null) continue;

            _fields.Add(new ConfigMenuField(_instance, field, entry));
        }
    }
    
    public GroupElementData GetElementData()
    {
        var group = new GroupElementData("Root");
        foreach (var field in _fields)
        {
            group.AddElement(field.GetElementData());
        }

        if (_instance is IConfigMenuProvider provider)
        {
            provider.AddExtraFields(group);
        }

        return group;
    }
}