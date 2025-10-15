namespace MashGamemodeLibrary.networking.Validation;

public interface INetworkRoute
{
    string GetName();

    bool CallOnSender()
    {
        return false;
    }
}