namespace MashGamemodeLibrary.networking.Control;

[AttributeUsage(AttributeTargets.Assembly)]
public class NetworkIdentifiable : Attribute
{
    public string Identifier { get; }
    public NetworkIdentifiable(string identifier)
    {
        Identifier = identifier;
    }
    
}