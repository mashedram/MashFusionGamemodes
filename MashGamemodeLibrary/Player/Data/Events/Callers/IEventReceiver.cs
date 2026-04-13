namespace MashGamemodeLibrary.Player.Data.Events.Callers;

public interface IEventReceiver
{
    void ReceiveEvent(IPlayerEvent playerEvent);
}