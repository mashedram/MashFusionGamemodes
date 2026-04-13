using Il2CppSLZ.VRMK;

namespace MashGamemodeLibrary.Player.Data.Events;

public record AvatarChangedEvent(Avatar Avatar) : IPlayerEvent;