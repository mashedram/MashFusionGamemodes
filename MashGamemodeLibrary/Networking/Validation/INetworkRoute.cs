namespace MashGamemodeLibrary.networking.Validation;

public interface INetworkRoute
{
    string GetName();

    bool ValidFromSender(byte id);

    bool CallOnSender()
    {
        return false;
    }
}