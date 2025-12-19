namespace MashGamemodeLibrary.Config.Menu.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class ConfigMenuEntry : Attribute
{
    public ConfigMenuEntry(string name, string? category = null)
    {
        Name = name;
        Category = category;
    }
    public string Name { get; }
    public string? Category { get; }
}