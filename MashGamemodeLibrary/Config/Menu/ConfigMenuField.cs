using System.Reflection;
using LabFusion.Marrow.Proxies;
using LabFusion.Menu.Data;
using MashGamemodeLibrary.Config.Constraints;
using MashGamemodeLibrary.Config.Menu.Attributes;
using MashGamemodeLibrary.Config.Menu.Element;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Config.Menu;

public class ConfigMenuField
{
    private IConfig _instance;
    
    private readonly string _name;
    public string? Category { get; private init; }
    private readonly FieldInfo _fieldInfo;
    private Type _fieldType;
    // TODO: Add default value reseting
    private object _defaultValue;

    private readonly IConfigElementProvider? _elementProvider;

    private readonly object? _incrementStep;

    private readonly ConfigRangeConstraint? _bounds;

    private readonly bool _synced = false;

    public ConfigMenuField(IConfig instance, FieldInfo field, ConfigMenuEntry entry)
    {
        _instance = instance;
        
        _name = entry.Name;
        Category = entry.Category;

        _fieldInfo = field;
        _fieldType = field.FieldType;

        var element = field.GetCustomAttribute<ConfigElementProvider>();
        if (element != null)
        {
            _elementProvider = (IConfigElementProvider?)Activator.CreateInstance(element.ProviderType);
        }

        var increment = field.GetCustomAttribute<ConfigStepSize>();
        if (increment != null)
        {
            if (increment.StepSize.GetType() != _fieldType)
            {
                MelonLogger.Warning($"Can't step {_name} by {increment.GetType().Name}. It is not of the same type as the field. ({_fieldType})");
            }
            else
            {
                _incrementStep = increment.StepSize;
            }
        }
        
        _bounds = field.GetCustomAttribute<ConfigRangeConstraint>();

        _defaultValue = _fieldInfo.GetValue(instance) ?? throw new Exception($"Config fields must be initialized ({_fieldInfo.Name})");

        _synced = field.GetCustomAttribute<SerializableField>() != null;
    }

    private T ReadValue<T>(T fallback) where T : notnull
    {
        return (T?)_fieldInfo.GetValue(_instance) ?? fallback;
    }

    private void WriteValue<T>(T value) where T : notnull
    {
        _fieldInfo.SetValue(_instance, value);
        
        if (_synced)
            ConfigManager.Sync();
    }

    public ElementData GetElementData()
    {
        if (_elementProvider != null)
        {
            return _elementProvider.GetElementData(_name, ReadValue<object>(null!), WriteValue);
        }
        
        // TODO: Use a registry
        if (_fieldType == typeof(float))
        {
            var min = (float?)_bounds?.Lower ?? 1f;
            var max = (float?)_bounds?.Upper ?? 10f;
            
            return new FloatElementData
            {
                Title = _name,
                Increment = (float?)_incrementStep ?? 1f,
                MinValue = min,
                MaxValue = max,
                Value = ReadValue(min),
                OnValueChanged = WriteValue
            };
        }

        if (_fieldType == typeof(bool))
        {
            return new BoolElementData
            {
                Title = _name,
                Value = (bool?)_fieldInfo.GetValue(_instance) ?? false,
                OnValueChanged = WriteValue
            };
        }

        if (_fieldType == typeof(int))
        {
            var min = (int?)_bounds?.Lower ?? 1;
            var max = (int?)_bounds?.Upper ?? 10;
            
            return new IntElementData
            {
                Title = _name,
                Increment = (int?)_incrementStep ?? 1,
                MinValue = min,
                MaxValue = max,
                Value = ReadValue(min),
                OnValueChanged = WriteValue
            };
        }

        if (typeof(Enum).IsAssignableFrom(_fieldType))
        {
            return new EnumElementData
            {
                Title = _name,
                EnumType = _fieldType,
                Value = (Enum)ReadValue(Activator.CreateInstance(_fieldType)!),
                OnValueChanged = WriteValue
            };
        }

        return new LabelElementData {Title = $"Field {_name} with {_fieldType.Name} is not supported."};
    }
}