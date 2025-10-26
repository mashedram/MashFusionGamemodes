using LabFusion.Menu.Data;
using LabFusion.Network.Serialization;
using LabFusion.Player;

namespace Clockhunt.Nightmare.Config;

public class NightmareConfig : INetSerializable
{
    public string? AvatarOverride = null;
    public float AbilityCooldown = 60f;

    protected virtual void SerializeCustom(INetSerializer serializer) {}

    // Just do 4096 and reduce the size.
    // Configs get send only once in a while anyway
    public int? GetSize()
    {
        return 4096;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref AvatarOverride);
        serializer.SerializeValue(ref AbilityCooldown);
        SerializeCustom(serializer);
    }
    
    public void AttachToGroup(GroupElementData group)
    {
        group.AddElement(new FunctionElementData
        {
            Title = "Set Override Avatar",
            OnPressed = () =>
            {
                AvatarOverride = LocalAvatar.AvatarBarcode;
            }
        });
        group.AddElement(new FunctionElementData
        {
            Title = "Clear Avatar",
            OnPressed = () =>
            {
                AvatarOverride = null;
            }
        });
        group.AddElement(new FloatElementData
        {
            Title = "Ability Cooldown",
            MaxValue = 120f,
            MinValue = 5f,
            Increment = 5f,
            Value = AbilityCooldown,
            OnValueChanged = value => AbilityCooldown = value
        });
    }
}