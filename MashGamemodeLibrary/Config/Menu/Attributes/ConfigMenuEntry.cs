namespace MashGamemodeLibrary.Config.Menu;

[AttributeUsage(AttributeTargets.Field)]
public class ConfigMenuEntry : Attribute
{
    public string Name { get; }
    public ConfigMenuEntry(string name)
    {
        Name = name;
    }
}