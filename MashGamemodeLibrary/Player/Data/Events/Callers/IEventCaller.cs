using LabFusion.Entities;

namespace MashGamemodeLibrary.Player.Data.Events.Callers;

public interface IEventCaller
{
    void OnEnable(IEventReceiver eventReceiver, NetworkPlayer networkPlayer);
    void OnDisable();
}