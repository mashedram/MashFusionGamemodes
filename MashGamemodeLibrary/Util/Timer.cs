namespace MashGamemodeLibrary.Util;

public enum MarkerType
{
    AfterStart,
    BeforeEnd,
    Interval
}

public class TimeMarker
{
    public bool Hit;

    public MarkerType Type;
    public float Time;
    public Action<float> OnHit;
    
    public TimeMarker(MarkerType type, float time, Action<float> onHit)
    {
        Type = type;
        Time = time;
        OnHit = onHit;
    }
}

public class Timer
{
    private TimeMarker[] _markers;

    private float _timeout;

    private bool _hitTimeout = false;
    private float _timer;
    
    public event Action? OnReset;
    public event Action<float>? OnTimeout;
    
    public Timer(float timeout, params TimeMarker[] markers)
    {
        _timeout = timeout;
        _markers = markers;
    }

    private float GetActualTime(TimeMarker marker)
    {
        var markerTime = marker.Time;
        return marker.Type switch
        {
            MarkerType.AfterStart => markerTime,
            MarkerType.BeforeEnd => _timeout - markerTime,
            _ => markerTime
        };
    }

    private void HandleIntervalMarker(TimeMarker marker, float delta)
    {
        var previousTime = _timer - delta;

        var interval = marker.Time;
        var currentFloored = MathF.Floor(_timer / interval);
        var previousFloored = MathF.Floor(previousTime / interval);

        var difference = MathF.Abs(currentFloored - previousFloored);
        if (difference == 0) return;
        
        marker.OnHit.Invoke(MathF.Floor(_timer));
    }

    private void HandleOffsetMarker(TimeMarker marker)
    {
        var markerTime = GetActualTime(marker);
        if (_timer < markerTime)
        {
            marker.Hit = false;
            return;
        }
        if (marker.Hit) return;
            
        marker.Hit = true;
        marker.OnHit.Invoke(markerTime);
    }

    private void CheckMarker(float delta)
    {
        foreach (var marker in _markers)
        {
            if (marker.Type is MarkerType.AfterStart or MarkerType.BeforeEnd)
                HandleOffsetMarker(marker);
            else
                HandleIntervalMarker(marker, delta);
        }
    }

    public void Update(float delta)
    {
        if (_hitTimeout) return;
        
        if (_timer > _timeout)
        {
            OnTimeout?.Invoke(_timeout);
            _hitTimeout = true;
            return;
        }
        
        _timer += delta;
        
        CheckMarker(delta);
    }

    public void SetTimeout(float timeout)
    {
        _timeout = timeout;

        var didHitTimeout = _hitTimeout;
        _hitTimeout = _timer >= _timeout;
        
        if (!didHitTimeout && _hitTimeout)
            OnTimeout?.Invoke(_timeout);
    }

    public bool HasReachedTimeout()
    {
        return _hitTimeout;
    }

    public float GetElapsedTime()
    {
        return _timer;
    }

    public void SetMarkers(params TimeMarker[] markers)
    {
        _markers = markers;
    }

    public void Reset()
    {
        if (_timer != 0f)
        {
            OnReset?.Invoke();
            
            foreach (var marker in _markers)
            {
                marker.Hit = false;
            }
        }
        
        _timer = 0f;
        _hitTimeout = false;
    }
}