using System.Numerics;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Colliders.Caches;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Caches.Containers;


internal struct CacheFrame<TValue>
{
    public const int FrameSize = sizeof(ulong);

    public TValue?[] Values { get; }
    private ulong Bitmask { get; set; }
    
    public CacheFrame()
    {
        Values = new TValue[FrameSize];
    }
    
    private static int GetFirstZeroBitIndex(ulong value)
    {
        return BitOperations.TrailingZeroCount(~value);
    }
    
    public readonly bool IsFull => Bitmask == ulong.MaxValue;
    
    public bool TryAdd(TValue value)
    {
        if (IsFull)
            return false;
        
        var index = GetFirstZeroBitIndex(Bitmask);
        Values[index] = value;
        Bitmask |= 1UL << index;
        return true;
    }
    
    public Span<TValue?> GetValues()
    {
        return Values;
}

public class FramedColliderCache
{
    private readonly LinkedList<CacheFrame<ICachedCollider>> _frames = new();
    
}