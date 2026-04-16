using Il2CppSLZ.VRMK;

namespace MashGamemodeLibrary.Player.Data.Events.Data;

public record AvatarChangedEvent(Avatar Avatar) : IPlayerEvent;