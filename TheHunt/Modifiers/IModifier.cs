namespace TheHunt.Modifiers;

public interface IModifier
{
    string Name { get; }
    void Apply();
    void Remove();
}