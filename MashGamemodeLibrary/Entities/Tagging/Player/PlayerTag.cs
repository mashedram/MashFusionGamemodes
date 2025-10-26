using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Tagging.Base;

namespace MashGamemodeLibrary.Entities.Tagging.Player;

public class PlayerTag : EntityTag, ITagAddedInternal
{
    protected NetworkPlayer Owner { get; private set; } = null!;
    
    public new void OnAddInternal(EntityTagIndex tag)
    {
        if (!NetworkPlayerManager.TryGetPlayer((byte)tag.EntityID, out var player))
        {
            throw new ArgumentException($"ID: {tag.EntityID} is not a player");
        }

        Owner = player;
        base.OnAddInternal(tag);
    }
}