using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.RPC;

namespace MashGamemodeLibrary.Entities;

public delegate void OnMarrowEntitySpawned(NetworkEntity networkEntity, MarrowEntity entity);

internal record CallbackHolder(ushort EntityId, OnMarrowEntitySpawned Callback)
{
    public NetworkEntity? NetworkEntity { get; init; }
    public int Tries { get; set; }
}

// TODO: Patch into MarrowEntity constructor and callback from there
public static class SpawnHelper
{
    private static LinkedListNode<CallbackHolder>? _currentNode;
    private static readonly LinkedList<CallbackHolder> TrackedEntities = new();

    public static void WaitOnMarrowEntity(this NetworkAssetSpawner.SpawnCallbackInfo callbackInfo, OnMarrowEntitySpawned callback)
    {
        WaitOnMarrowEntity(callbackInfo.Entity, callback);
    }

    public static void WaitOnMarrowEntity(NetworkEntity networkEntity, OnMarrowEntitySpawned callback)
    {
        WaitOnMarrowEntity(networkEntity.ID, networkEntity, callback);
    }

    public static void WaitOnMarrowEntity(ushort entityId, OnMarrowEntitySpawned callback)
    {
        var networkEntity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(entityId);
        WaitOnMarrowEntity(entityId, networkEntity, callback);
    }

    private static void WaitOnMarrowEntity(ushort entityId, NetworkEntity? networkEntity, OnMarrowEntitySpawned callback)
    {
        // Ensure the NetworkEntity is valid
        if (networkEntity != null)
        {
            if (entityId == networkEntity.ID)
            {
                // Check early
                var extension = networkEntity.GetExtender<IMarrowEntityExtender>();
                if (extension != null)
                {
                    callback(extension.NetworkEntity, extension.MarrowEntity);
                    return;
                }
            }
            else
            {
                networkEntity = null;
            }
        }

        TrackedEntities.AddLast(new CallbackHolder(entityId, callback)
        {
            NetworkEntity = networkEntity
        });
    }

    private static void CheckEntity(LinkedListNode<CallbackHolder> node)
    {
        var networkEntity = node.Value.NetworkEntity;

        if (networkEntity == null)
        {
            if (!NetworkEntityManager.IDManager.RegisteredEntities.IDEntityLookup.TryGetValue(node.Value.EntityId, out networkEntity))
                return;
        }

        if (networkEntity.IsDestroyed)
        {
            TrackedEntities.Remove(node);
            return;
        }

        var extender = networkEntity.GetExtender<IMarrowEntityExtender>();
        if (extender == null)
            return;


        TrackedEntities.Remove(node);
        node.Value.Callback(extender.NetworkEntity, extender.MarrowEntity);
    }

    internal static void Update()
    {
        if (TrackedEntities.Count == 0)
            return;

        const int entriesPerIndex = 2;
        const int maxRetries = 50;

        for (var i = 0; i < entriesPerIndex; i++)
        {
            _currentNode ??= TrackedEntities.First;

            if (_currentNode == null)
                return;

            var nextNode = _currentNode.Next;

            _currentNode.Value.Tries += 1;
            CheckEntity(_currentNode);

            if (_currentNode is { List: not null, Value.Tries: > maxRetries })
            {
                TrackedEntities.Remove(_currentNode);
            }

            _currentNode = nextNode ?? TrackedEntities.First;
        }
    }
}