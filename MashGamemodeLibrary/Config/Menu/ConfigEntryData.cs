using System.Reflection;
using LabFusion.Marrow.Proxies;
using LabFusion.Menu.Data;
using MashGamemodeLibrary.Config.Menu.Attributes;
using MashGamemodeLibrary.Config.Menu.Element;
using MashGamemodeLibrary.Registry.Keyed;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Config.Menu;

public struct Bounds
{
    public object Lower;
    public object Upper;
}

public record ConfigEntryData
{
    public bool Visible;
    public readonly FieldInfo FieldInfo;

    public readonly string Name;
    public readonly string? Category;
    public readonly Type Type;
    public readonly object DefaultValue;

    private IConfigElementProvider? _elementProvider;
    private ElementData? _cachedElementData;

    public object? Increment;
    public Bounds? Bounds;

    private object? _overwrite;

    public object Value => _overwrite ?? DefaultValue;

    private static readonly KeyedRegistry<Type, IConfigElementProvider> ElementProviders = new();

    static ConfigEntryData()
    {
        ElementProviders.Register(typeof(float), new FloatElementProvider());
        ElementProviders.Register(typeof(int), new IntElementProvider());
        ElementProviders.Register(typeof(bool), new BoolElementProvider());
        ElementProviders.Register(typeof(Enum), new EnumElementProvider());
    }

    private static Type GetBaseType(Type type)
    {
        if (type.IsEnum)
            return typeof(Enum);
        
        return type;
    }

    public ConfigEntryData(IConfig instance, FieldInfo fieldInfo)
    {
        FieldInfo = fieldInfo;
        Type = fieldInfo.FieldType;

        var menuEntry = fieldInfo.GetCustomAttribute<ConfigMenuEntry>();
        if (menuEntry == null)
        {
            Visible = false;
            return;
        }

        Visible = true;
        Name = menuEntry.Name;
        Category = menuEntry.Category;

        ApplyConfigElementProvider(fieldInfo);
        ApplyIncrement(fieldInfo);
        ApplyBounds(fieldInfo);

        DefaultValue = fieldInfo.GetValue(instance) ??
                       throw new Exception($"Config fields must be initialized ({fieldInfo.Name})");
    }
    
    private Action<ConfigEntryData, object> WriterFactory(IConfig config)
    {
        return (target, value) =>
        {
            target._overwrite = value;
            target.FieldInfo.SetValue(config, value);

            ConfigManager.OnValueChanged();
        };
    }

    private ElementData CreateElementData(IConfig instance)
    {
        if (_elementProvider != null)
        {
            return _elementProvider.GetElementData(this, WriterFactory(instance));
        }

        if (ElementProviders.TryGet(GetBaseType(Type), out var provider))
        {
            return provider.GetElementData(this, WriterFactory(instance));
        }

        return new LabelElementData
        {
            Title = $"Field type {Type.Name} has no associated element provider."
        };
    }
    
    public ElementData GetElementData(IConfig instance)
    {
        _cachedElementData ??= CreateElementData(instance);

        return _cachedElementData;
    }

    private void ApplyConfigElementProvider(FieldInfo fieldInfo)
    {
        var element = fieldInfo.GetCustomAttribute<ConfigElementProvider>();
        if (element == null)
            return;

        _elementProvider = (IConfigElementProvider?)Activator.CreateInstance(element.ProviderType);
    }


    private void ApplyIncrement(FieldInfo fieldInfo)
    {
        var stepAttribute = fieldInfo.GetCustomAttribute<ConfigStepSize>();
        if (stepAttribute == null)
            return;

        if (stepAttribute.StepSize.GetType() != Type)
        {
            MelonLogger.Warning(
                $"Can't step {Name} by {stepAttribute.StepSize.GetType().Name}. It is not of the same type as the field. ({Type})");
            return;
        }

        Increment = stepAttribute.StepSize;
    }

    private void ApplyBounds(FieldInfo fieldInfo)
    {
        var boundsAttribute = fieldInfo.GetCustomAttribute<ConfigRangeConstraint>();
        if (boundsAttribute == null)
            return;

        if (boundsAttribute.Lower.GetType() != Type)
        {
            MelonLogger.Warning(
                $"Can't lower bound {Name} by {boundsAttribute.Lower.GetType().Name}. It is not of the same type as the field. ({Type})");
            return;
        }

        if (boundsAttribute.Upper.GetType() != Type)
        {
            MelonLogger.Warning(
                $"Can't upper bound {Name} by {boundsAttribute.Upper.GetType().Name}. It is not of the same type as the field. ({Type})");
            return;
        }

        Bounds = new Bounds
        {
            Lower = boundsAttribute.Lower,
            Upper = boundsAttribute.Upper
        };
    }
}