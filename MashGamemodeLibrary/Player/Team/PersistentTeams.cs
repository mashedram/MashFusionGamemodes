using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Data.Random;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Player.Actions;
using MelonLoader;
using Random = UnityEngine.Random;

namespace MashGamemodeLibrary.Player.Team;

public enum TeamStatisticKeys
{
    RoundsWon
}

public static class PersistentTeams
{
    private static readonly RemoteEvent WinMessageEvent =
        new("PersistentTeams_WinMessage", OnWinMessage,
            CommonNetworkRoutes.HostToAll);
    private static readonly SyncedDictionary<byte, int> PlayerTeamIndices = 
        new("PersistentTeams_PlayerTeamIndices", new ByteEncoder(), new IntEncoder());
    private static readonly SyncedDictionary<int, int> TeamScores = 
        new("PersistentTeams_TeamScores", new IntEncoder(), new IntEncoder());

    private static readonly HashSet<PlayerID> LateJoinerQueue = new();
    private static readonly List<ulong> TeamIds = new();

    private static int _shift = Random.RandomRangeInt(0, 2);

    private static ulong GetTeamId(int setIndex)
    {
        var index = (setIndex + _shift) % TeamIds.Count;
        return TeamIds[index];
    }

    public static void AddTeamID(ulong id)
    {
        TeamIds.Add(id);
    }

    public static void AddTeam<T>() where T : LogicTeam
    {
        var id = LogicTeamManager.Registry.CreateID<T>();
        AddTeamID(id);
    }

    private static void Assign(PlayerID playerID, int index)
    {
        // Avoid double adding
        if (PlayerTeamIndices.ContainsKey(playerID))
            return;

        PlayerTeamIndices[playerID] = index;
    }

    public static void AddPlayers(IEnumerable<PlayerID> playerIds)
    {
        var index = 0;
        foreach (var playerID in playerIds.Shuffle())
        {
            Assign(playerID, index);
            index = (index + 1) % TeamIds.Count;
        }
    }
    
    public static void OverwritePlayers(IEnumerable<PlayerID> playerIds)
    {
        PlayerTeamIndices.Clear();
        AddPlayers(playerIds);
    }
    
    public static void OverwritePlayers(IEnumerable<IEnumerable<PlayerID>> teamPlayerIds)
    {
        PlayerTeamIndices.Clear();
        var index = 0;
        foreach (var playerId in teamPlayerIds.SelectMany(t => t))
        {
            Assign(playerId, index);
            index = (index + 1) % TeamIds.Count;
        }
    }

    public static void RandomizeShift()
    {
        _shift += Random.RandomRangeInt(0, TeamIds.Count);
    }

    public static void QueueLateJoiner(PlayerID playerID)
    {
        LateJoinerQueue.Add(playerID);
    }

    public static void AssignAll()
    {
        if (PlayerTeamIndices.Count == 0)
        {
            MelonLogger.Error("No valid set found.");
            return;
        }

        // Resolve queue
        var teamSizes = PlayerTeamIndices
            .Select(p => p.Value)
            .GroupBy(i => i)
            .ToDictionary(g => g.Key, g => g.Count());
        foreach (var playerID in LateJoinerQueue)
        {
            if (!playerID.IsValid)
            {
                LateJoinerQueue.Remove(playerID);
                continue;
            }

            if (!NetworkPlayerManager.TryGetPlayer(playerID, out var player))
                continue;

            if (!player.HasRig)
                continue;

            LateJoinerQueue.Remove(playerID);

            var smallestTeamIndex = teamSizes.MinBy(t => t.Value).Key;
            Assign(playerID, smallestTeamIndex);
            teamSizes[smallestTeamIndex] += 1;
        }

        // Assign teams
        _shift += 1;
        
        foreach (var (smallId, teamIndex) in PlayerTeamIndices)
        {
            var playerId = PlayerIDManager.GetPlayerID(smallId);
            
            if (!playerId.IsValid)
                return;
            
            playerId.Assign(GetTeamId(teamIndex));
        }
    }

    public static void AddScore(ulong teamId, int score)
    {
        var index = TeamIds.IndexOf(teamId);
        var teamIndex = (index - _shift) % TeamIds.Count;
        if (teamIndex < 0)
            teamIndex += TeamIds.Count;

        var currentScore = TeamScores.GetValueOrDefault(teamIndex, 0);
        TeamScores[teamIndex] = currentScore + score;
    }
    
    public static void Clear()
    {
        _shift = 0;
        TeamIds.Clear();
        PlayerTeamIndices.Clear();
        TeamScores.Clear();
    }
    
    public static int GetTeamIndex(PlayerID playerID)
    {
        return PlayerTeamIndices.TryGetValue(playerID, out var index) ? index : -1;
    }
    
    public static int GetTeamScore(int teamIndex)
    {
        return TeamScores.TryGetValue(teamIndex, out var score) ? score : 0;
    }
    
    public static int GetPlayerTeamScore(PlayerID playerID)
    {
        var teamIndex = GetTeamIndex(playerID);
        return GetTeamScore(teamIndex);
    }

    public static void SendMessage()
    {
        if (PlayerTeamIndices.Count == 0)
            return;

        WinMessageEvent.Call();
    }
    

    // events 
    private static void OnWinMessage(byte senderId)
    {
        var finals = new List<(string, int)>(2);
        var teamScores = TeamScores
            .Select(kvp => (TeamId: kvp.Key, score: kvp.Value))
            .OrderByDescending(p => p.score)
            .Take(2)
            .ToList();

        if (teamScores.Count == 0)
            return;

        foreach (var (teamID, score) in teamScores)
        {
            var playerID = PlayerTeamIndices.FirstOrDefault(p => p.Value == teamID).Key;
            var name = NetworkPlayerManager.TryGetPlayer(playerID, out var player)
                ? player.Username
                : "Unknown";

            finals.Add((name, score));
        }

        var localTeamID = PlayerTeamIndices[PlayerIDManager.LocalSmallID];
        var localWinner = teamScores.First().TeamId == localTeamID;

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

        PlayerStatisticsTracker.AwardBits(bits, PlayerDamageStatistics.Kills, PlayerDamageStatistics.Assists,
            PlayerDamageStatistics.Deaths);
    }
}