namespace MashGamemodeLibrary.Config.Menu;

[AttributeUsage(AttributeTargets.Field)]
public class ConfigMenuEntry : Attribute
{
    public string Name { get; }
    public string? Category { get; }
    public ConfigMenuEntry(string name, string? category = null)
    {
        Name = name;
        Category = category;
    }
}