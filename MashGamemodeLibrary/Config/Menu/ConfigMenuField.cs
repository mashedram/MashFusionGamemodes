using System.Reflection;
using LabFusion.Marrow.Proxies;
using LabFusion.Menu.Data;
using MashGamemodeLibrary.Config.Constraints;
using MashGamemodeLibrary.Config.Menu.Element;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Config.Menu;

class RangeBounds
{
    private object _lower;
    private object _higher;
}

class DirectDisplayTransformer : IConfigDisplayTransformer
{
    public object ToDisplay(object value)
    {
        return value;
    }
    public object FromDisplay(object display)
    {
        return display;
    }
}

public class ConfigMenuField
{
    private IConfig _instance;
    
    private string Name;
    private FieldInfo fieldInfo;
    private object DefaultValue;
    
    private Type DisplayType;
    private IConfigDisplayTransformer displayTransformer;

    private object? _incrementStep;

    private ConfigRangeConstraint? _bounds;

    private bool _synced = false;

    public ConfigMenuField(IConfig instance, FieldInfo field, ConfigMenuEntry entry)
    {
        _instance = instance;
        
        Name = entry.Name;

        fieldInfo = field;
        var fieldType = field.FieldType;
        
        var display = field.GetCustomAttribute<ConfigDisplayTransformer>();
        if (display != null)
        {
            DisplayType = display.DisplayType;
            displayTransformer = displayTransformer = (IConfigDisplayTransformer?)Activator.CreateInstance(display.TransformerType) ?? new DirectDisplayTransformer();
        }
        else
        {
            DisplayType = fieldType;
            displayTransformer = new DirectDisplayTransformer();
        }

        var increment = field.GetCustomAttribute<ConfigStepSize>();
        if (increment != null)
        {
            if (increment.StepSize.GetType() != fieldType)
            {
                MelonLogger.Warning($"Can't step {Name} by {increment.GetType().Name}. It is not of the same type as the field. ({fieldType})");
            }
            else
            {
                _incrementStep = increment.StepSize;
            }
        }
        
        _bounds = field.GetCustomAttribute<ConfigRangeConstraint>();

        _synced = field.GetCustomAttribute<Synchronise>() != null;
    }

    private T ToDisplayValue<T>(object? value, T fallback) where T : notnull
    {
        if (value == null)
            return fallback;
        
        return (T)displayTransformer.ToDisplay(value);
    }
    
    private T FromDisplayValue<T>(object? value, T fallback) where T : notnull
    {
        if (value == null)
            return fallback;
        
        return (T)displayTransformer.FromDisplay(value);
    }

    private T ReadValue<T>(T fallback) where T : notnull
    {
        return ToDisplayValue(fieldInfo.GetValue(_instance), fallback);
    }

    private void WriteValue<T>(T value) where T : notnull
    {
        fieldInfo.SetValue(_instance, displayTransformer.FromDisplay(value));
        
        if (_synced)
            ConfigManager.Sync();
    }

    public ElementData GetElementData()
    {
        // TODO: Use a registry
        if (DisplayType == typeof(float))
        {
            var min = ToDisplayValue(_bounds?.Lower, 1f);
            var max = ToDisplayValue(_bounds?.Upper, 10f);
            
            return new FloatElementData
            {
                Title = Name,
                Increment = ToDisplayValue(_incrementStep, 1f),
                MinValue = min,
                MaxValue = max,
                Value = ReadValue(min),
                OnValueChanged = WriteValue
            };
        }

        if (DisplayType == typeof(bool))
        {
            return new BoolElementData
            {
                Title = Name,
                Value = ToDisplayValue(fieldInfo.GetValue(_instance), false),
                OnValueChanged = WriteValue
            };
        }

        if (DisplayType == typeof(int))
        {
            var min = ToDisplayValue(_bounds?.Lower, 1);
            var max = ToDisplayValue(_bounds?.Upper, 10);
            
            return new IntElementData
            {
                Title = Name,
                Increment = ToDisplayValue(_incrementStep, 1),
                MinValue = min,
                MaxValue = max,
                Value = ReadValue(min),
                OnValueChanged = WriteValue
            };
        }

        if (typeof(Enum).IsAssignableFrom(DisplayType))
        {
            return new EnumElementData
            {
                Title = Name,
                EnumType = DisplayType,
                Value = (Enum)ReadValue(Activator.CreateInstance(DisplayType)!),
                OnValueChanged = WriteValue
            };
        }

        return new LabelElementData {Title = $"Field {Name} with {DisplayType.Name} is not supported."};
    }
}