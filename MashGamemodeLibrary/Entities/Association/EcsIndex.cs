using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Registry.Typed;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Entities.Association;

public class EcsIndex : INetSerializable, IEquatable<EcsIndex>
{
    public static readonly FactoryTypedRegistry<IEcsAssociation> AssociationRegistry = new();

    private ulong _componentID;
    private IEcsAssociation? _association;

    // Default constructor for serialization
    public EcsIndex() {}
    
    public EcsIndex(ulong componentID, IEcsAssociation? association = null)
    {
        _componentID = componentID;
        _association = association;
    }
    
    public EcsIndex(Type componentType, IEcsAssociation? association = null)
    {
        _componentID = EcsManager.ComponentRegistry.GetID(componentType);
        _association = association;
    }
    
    public EcsIndex(IComponent component, IEcsAssociation? association = null)
    {
        _componentID = EcsManager.ComponentRegistry.GetID(component);
        _association = association;
    }
    
    public ulong ComponentID => _componentID;
    public IEcsAssociation? Association => _association;

    public void HookCreation(Action action)
    {
        // No association, then we can create immediately
        if (_association == null)
        {
            action();
            return;
        }

        _association?.HookReady(action);
    }

    public void HookRemoval(Action action)
    {
        // No association = global resource, which will only be removed manually
        if (_association == null)
            return;

        _association.HookRemoval(action);
    }
    
    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _componentID);
        AssociationRegistry.SerializeValue(serializer, ref _association);
    }
    
    public bool Equals(EcsIndex? other)
    {
        // Null indexes are never equal
        if (other == null)
            return false;

        if (_componentID != other._componentID)
            return false;

        // If the ID is the same, and the associations are both null, this is the same global index
        if (_association == null && other._association == null)
            return true;

        // Check the associations hashes, if they are equal (They can not both be null, we checked this earlier), the index is the same
        return _association?.GetID() == other._association?.GetID();
    }
}