using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Data.Random;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Player.Actions;
using MelonLoader;
using Random = UnityEngine.Random;

namespace MashGamemodeLibrary.Player.Team;

internal class WinMessagePacket : INetSerializable
{
    public List<int> Scores;
    public List<(int, byte)> PlayerIds;
    
    public void Serialize(INetSerializer serializer)
    {
        if (serializer.IsReader)
        {
            var reader = (NetReader)serializer;
            var size = reader.ReadInt32();
            Scores = new List<int>(size);
            for (int i = 0; i < size; i++)
            {
                Scores.Add(reader.ReadInt32());
            }
            
            size = reader.ReadInt32();
            PlayerIds = new List<(int, byte)>(size);
            for (int i = 0; i < size; i++)
            {
                var score = reader.ReadInt32();
                var playerID = reader.ReadByte();
                PlayerIds.Add((score, playerID));
            }
            return;
        }
        
        var writer = (NetWriter)serializer;
        writer.Write(Scores.Count);
        foreach (var score in Scores)
        {
            writer.Write(score);
        }
        writer.Write(PlayerIds.Count);
        foreach (var (score, playerID) in PlayerIds)
        {
            writer.Write(score);
            writer.Write(playerID);
        }
    }
}

public class PersistentTeams
{
    private static readonly RemoteEvent<WinMessagePacket> WinMessageEvent =
        new("PersistentTeams_WinMessage", OnWinMessage,
            CommonNetworkRoutes.HostToAll);

    private int _shift = Random.RandomRangeInt(0, 2);
    private readonly List<ulong> _teamIds = new();
    private readonly HashSet<PlayerID> _playerIds = new();
    private readonly List<HashSet<PlayerID>> _playerSets = new();
    private readonly List<int> _scores = new();
    private readonly Queue<PlayerID> _lateJoinerQueue = new();
    
    private readonly 

    private ulong GetTeamId(int setIndex)
    {
        var index = (setIndex + _shift) % _teamIds.Count;
        return _teamIds[index];
    }

    public void AddTeamID(ulong id)
    {
        _teamIds.Add(id);
        _playerSets.Add(new HashSet<PlayerID>());
        _scores.Add(0);
    }

    public void AddTeam<T>() where T : Team
    {
        var id = TeamManager.Registry.CreateID<T>();
        AddTeamID(id);
    }

    public void AddPlayers(IEnumerable<PlayerID> playerIds)
    {
        _playerSets.ForEach(set => set.Clear());
        _playerIds.Clear();
        
        var index = 0;
        foreach (var playerID in playerIds.Shuffle())
        {
            _playerIds.Add(playerID);
            _playerSets[index].Add(playerID);
            index = (index + 1) % _playerSets.Count;
        }
    }

    public void RandomizeShift()
    {
        _shift += Random.RandomRangeInt(0, _teamIds.Count);
    }
    
    public void QueueLateJoiner(PlayerID playerID)
    {
        _lateJoinerQueue.Enqueue(playerID);
    } 

    public void AssignAll()
    {
        if (_playerSets.Count == 0)
        {
            MelonLogger.Error("No valid set found.");
            return;
        }
        
        // Resolve queue
        var index = _playerSets.Select((set, index) => (index, set)).MinBy(set => set.set.Count).index;
        while (_lateJoinerQueue.TryDequeue(out var playerID))
        {
            // Avoid double adding
            if (!_playerIds.Add(playerID))
                continue;
            
            _playerSets[index].Add(playerID);

            index = (index + 1) % _playerSets.Count;
        }
        
        // Assign teams
        _shift += 1;
        
        for (int i = 0; i < _playerSets.Count; i++)
        {
            var teamID = GetTeamId(i);
            foreach (var playerID in _playerSets[i])
            {
                playerID.Assign(teamID);
            }
        }
    }
    
    public void AddScore(ulong teamId, int score)
    {
        var index = _teamIds.IndexOf(teamId);
        var setIndex = (index - _shift) % _teamIds.Count;
        if (setIndex < 0)
            setIndex += _teamIds.Count;
        
        _scores[setIndex] += score;
    }

    public void SendMessage()
    {
        var playerIds = _playerSets
            .SelectMany((set, index) => set.Where(id => id.IsValid).Select(id => (index, id.SmallID))).ToList();
        
        if (playerIds.Count == 0)
            return;
        
        var packet = new WinMessagePacket
        {
            Scores = _scores,
            PlayerIds = playerIds
        };
        
        WinMessageEvent.Call(packet);
    }

    public void Clear()
    {
        _shift = 0;
        _teamIds.Clear();
        _playerSets.Clear();
        _scores.Clear();
    }
    
    // events 
    private static void OnWinMessage(WinMessagePacket packet)
    {
        var finals = new List<(string, int)>(2);
        var teamScores = packet.Scores
            .Select((score, teamID) => (teamID, score))
            .OrderByDescending(p => p.score)
            .Take(2)
            .ToList();
        
        if (teamScores.Count == 0)
            return;
        
        foreach (var (teamID, score) in teamScores)
        {
            var playerID = packet.PlayerIds.FirstOrDefault(p => p.Item1 == teamID).Item2;
            var name = NetworkPlayerManager.TryGetPlayer(playerID, out var player)
                ? player.Username
                : "Unknown";
            
            finals.Add((name, score));
        }

        var localTeamID = packet.PlayerIds.Where(p => p.Item2 == PlayerIDManager.LocalSmallID).Select(p => p.Item1).FirstOrDefault(-1);
        var localWinner = teamScores.First().teamID == localTeamID;
        
        var message = localWinner ? "Victory!" : "Defeat!";
        var detail = string.Join("\n", finals.Select(f => $"{f.Item1}'s Team: {f.Item2} points"));
        
        Notifier.Send(new Notification
        {
            Title = message,
            Message = detail,
            PopupLength = 6f,
            SaveToMenu = true,
            ShowPopup = true,
            Type = localWinner ? NotificationType.SUCCESS : NotificationType.ERROR
        });

        var winCount = teamScores[localTeamID].score;
        var bits = (localWinner && teamScores[localTeamID].score > 0 ? 100 : 0) + winCount * 20;
        
        PlayerStatisticsTracker.SendNotificationAndAwardBits(bits ,PlayerDamageStatistics.Kills, PlayerDamageStatistics.Assists);
    }
}