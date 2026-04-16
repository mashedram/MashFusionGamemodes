namespace MashGamemodeLibrary.Entities.Queries;

public record CacheKey(ICachedQuery Query, Guid Guid)
{
    private bool _isValid = true;

    public void Remove()
    {
        if (!_isValid)
            return;

        _isValid = false;
        Query.Remove(this);
    }
}