using LabFusion.Entities;
using MashGamemodeLibrary.Audio.Players.Extensions;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Entities.Tagging.Base;
using Random = UnityEngine.Random;

namespace MashGamemodeLibrary.Audio.Players.Background.Timed;

public class TimedTagPlayer<T> : IContinuousPlayer where T : IEntityTag
{
    private readonly Dictionary<ushort, float> _entityTimers;

    private readonly float _maxTimeBetweenPlays;
    private readonly float _minTimeBetweenPlays;
    private readonly IRandomObjectAudioPlayer _player;

    public TimedTagPlayer(IRandomObjectAudioPlayer player, float minTimeBetweenPlays, float maxTimeBetweenPlays)
    {
        _player = player;
        _minTimeBetweenPlays = minTimeBetweenPlays;
        _maxTimeBetweenPlays = maxTimeBetweenPlays;

        _entityTimers = new Dictionary<ushort, float>();
    }

    public bool IsActive { get; private set; }

    public void StartPlaying()
    {
        if (IsActive) return;
        IsActive = true;
        _entityTimers.Clear();
    }

    public void StopPlaying()
    {
        if (!IsActive) return;
        IsActive = false;

        _player.StopAll();
    }

    public void Update(float delta)
    {
        if (!IsActive)
            return;

        _player.Update(delta);

        var entities = EntityTagManager.GetAllIdsWithTag<T>();

        foreach (var (id, _) in _entityTimers)
        {
            if (entities.Contains(id)) continue;
            _entityTimers.Remove(id);
        }

        foreach (var id in entities)
        {
            if (!_entityTimers.ContainsKey(id))
            {
                _entityTimers.Add(id, GetRandomTimeBetweenPlays());
                return;
            }

            _entityTimers[id] = Math.Max(0, _entityTimers[id] - delta);
            if (_entityTimers[id] > 0) continue;
            _entityTimers[id] = GetRandomTimeBetweenPlays();

            if (!new NetworkEntityReference(id).TryGetEntity(out var entity))
            {
                _entityTimers.Remove(id);
                return;
            }

            _player.PlayRandomAt(entity);
        }
    }

    private float GetRandomTimeBetweenPlays()
    {
        return Random.Range(_minTimeBetweenPlays, _maxTimeBetweenPlays);
    }
}