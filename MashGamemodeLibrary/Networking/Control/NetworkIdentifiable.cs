namespace MashGamemodeLibrary.networking.Control;

[AttributeUsage(AttributeTargets.Assembly)]
public class NetworkIdentifiable : Attribute
{
    public NetworkIdentifiable(string identifier)
    {
        Identifier = identifier;
    }
    public string Identifier { get; }
}