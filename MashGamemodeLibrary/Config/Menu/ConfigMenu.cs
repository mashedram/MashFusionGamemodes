using System.Reflection;
using LabFusion.Menu.Data;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Config.Menu;

public class ConfigMenu
{
    private readonly IConfig _instance;
    private readonly List<ConfigEntryData> _fields = new();

    public ConfigMenu(IConfig instance)
    {
        _instance = instance;

        var configType = instance.GetType();
        var fields = configType.GetFields();
        
        foreach (var field in fields)
        {
            var entry = field.GetCustomAttribute<ConfigMenuEntry>();
            if (entry == null) continue;

            _fields.Add(new ConfigEntryData(instance, field));
        }
    }
    
    public GroupElementData GetElementData()
    {
        var root = new GroupElementData("Root");
        var groups = new Dictionary<string, GroupElementData>();
        foreach (var field in _fields)
        {
            var group = field.Category != null ? groups.GetOrCreate(field.Category, () => new GroupElementData(field.Category)) : root;
            var elementData = field.GetElementData(_instance);
            
            group.AddElement(elementData);
        }
        
        foreach (var groupElementData in groups.Values)
        {
            root.AddElement(groupElementData);
        }

        if (_instance is IConfigMenuProvider provider)
        {
            provider.AddExtraFields(root);
        }

        return root;
    }
}