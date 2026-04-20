using MashGamemodeLibrary.Audio.Players.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MashGamemodeLibrary.Audio.Players.Background.Timed;

public class TimedPositionalPlayer : IContinuousPlayer
{
    private readonly float _maxTimeBetweenPlays;
    private readonly float _minTimeBetweenPlays;
    private readonly IParameterDriven<Vector3> _player;
    private readonly Func<Vector3> _positionProvider;
    
    private float _delayTimer;

    public TimedPositionalPlayer(IParameterDriven<Vector3> player, Func<Vector3> positionProvider, float minTimeBetweenPlays, float? maxTimeBetweenPlays = null)
    {
        _player = player;
        _positionProvider = positionProvider;
        _minTimeBetweenPlays = minTimeBetweenPlays;
        _maxTimeBetweenPlays = maxTimeBetweenPlays ?? minTimeBetweenPlays;
    }

    public bool IsActive { get; private set; }

    public void Start()
    {
        if (IsActive) return;

        IsActive = true;
        _delayTimer = GetRandomTimeBetweenPlays();
    }

    public void Stop()
    {
        if (!IsActive) return;

        IsActive = false;

        _player.Stop();
    }

    public void Update(float delta)
    {
        if (!IsActive)
            return;

        _player.Update(delta);

        if (_delayTimer > 0f)
        {
            _delayTimer -= delta;
            return;
        }

        _delayTimer = GetRandomTimeBetweenPlays();
        var position = _positionProvider.Invoke();
        _player.PlayRandom(position);
    }

    private float GetRandomTimeBetweenPlays()
    {
        if (Math.Abs(_minTimeBetweenPlays - _maxTimeBetweenPlays) < 1f)
            return _minTimeBetweenPlays;

        return Random.Range(_minTimeBetweenPlays, _maxTimeBetweenPlays);
    }
}