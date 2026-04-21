using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using Il2CppTMPro;
using LabFusion.Player;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Player.Actions;
using MashGamemodeLibrary.Player.Team;
using UnityEngine;
using UnityEngine.UI;

namespace BoneStrike.Manager;

internal class LeaderboardPlayerEntry
{
    public GameObject Root;
    public Image Background;
    public TextMeshPro PositionText;
    public TextMeshPro NameText;

    public TextMeshPro KillsText;
    public TextMeshPro DeathsText;
    public TextMeshPro AssistsText;
    public TextMeshPro WinsText;
    public TextMeshPro ScoreText;

    public LeaderboardPlayerEntry(GameObject root)
    {
        var transform = root.transform;
        Root = transform.Find("Content").gameObject;
        Background = Root.GetComponent<Image>();

        PositionText = root.transform.Find("Content/Position").GetComponent<TextMeshPro>();
        NameText = root.transform.Find("Content/Player").GetComponent<TextMeshPro>();
        KillsText = root.transform.Find("Content/Stats/Kills").GetComponent<TextMeshPro>();
        DeathsText = root.transform.Find("Content/Stats/Deaths").GetComponent<TextMeshPro>();
        AssistsText = root.transform.Find("Content/Stats/Assists").GetComponent<TextMeshPro>();
        WinsText = root.transform.Find("Content/Stats/Wins").GetComponent<TextMeshPro>();
        ScoreText = root.transform.Find("Content/Stats/Score").GetComponent<TextMeshPro>();
    }
}

internal class LeaderboardPlayerData
{
    public PlayerID PlayerId { get; init; }
    public int Kills { get; init; }
    public int Deaths { get; init; }
    public int Assists { get; init; }
    public int Wins { get; init; }
    public int Score { get; init; }

    public LeaderboardPlayerData(PlayerStatistics statistics)
    {
        PlayerId = PlayerIDManager.GetPlayerID(statistics.PlayerID);
        Kills = statistics.GetValue(PlayerDamageStatistics.Kills);
        Deaths = statistics.GetValue(PlayerDamageStatistics.Deaths);
        Assists = statistics.GetValue(PlayerDamageStatistics.Assists);
        Wins = statistics.GetValue(TeamStatisticKeys.RoundsWon);

        Score = (Kills + Assists / 2 + Wins * 2 - Deaths) * 125;
    }
}

// This components can't handle lol
public static class LeaderboardManager
{
    private const string Barcode = "Mash.BoneStrike.Spawnable.Leaderboard";
    private static Poolee? _poolee;
    private static readonly List<LeaderboardPlayerEntry> Entries = new();

    private static void Spawn(Vector3 position)
    {
        if (_poolee != null)
        {
            SetContent();
            return;
        }
        
        GameAssetSpawner.SpawnLocalAsset(Barcode, position, poolee =>
        {
            _poolee = poolee;
            LoadEntries();
            SetContent();
        });
    }
    
    private static void SetEntryData(LeaderboardPlayerEntry entry, LeaderboardPlayerData data, int position)
    {
        entry.PositionText.text = position.ToString();
        var nickname = data.PlayerId.Metadata.Nickname.GetValueOrEmpty();
        var name = string.IsNullOrEmpty(nickname) ? data.PlayerId.Metadata.Username.GetValueOrEmpty() : nickname;
        entry.NameText.text = name;
        entry.KillsText.text = data.Kills.ToString();
        entry.DeathsText.text = data.Deaths.ToString();
        entry.AssistsText.text = data.Assists.ToString();
        entry.WinsText.text = data.Wins.ToString();
        entry.ScoreText.text = data.Score.ToString();

        entry.Background.color = PersistentTeams.GetTeamIndex(data.PlayerId) == 0 ? new Color(1f, 0.2f, 0.2f) : new Color(0.2f, 0.2f, 1f);
    }

    public static void SetContent()
    {
        if (_poolee == null)
            return;
        
        var statistics = GlobalStatisticsCollector.Statistics
            .Select(v => new LeaderboardPlayerData(v))
            .Where(v => v.PlayerId.IsValid)
            .ToList();
        statistics.Sort((a, b) => b.Score.CompareTo(a.Score));

        var hasAssignedLocalPlayer = false;
        // We need to skip the header, thus the -1
        for (var i = 0; i < Entries.Count - 1; i++)
        {
            var entry = Entries[i + 1];

            // Check visibility
            var isVisible = i < statistics.Count;
            entry.Root.SetActive(isVisible);
            if (!isVisible)
                continue;

            var data = statistics[i];
            SetEntryData(entry, data, i + 1);
        }

        if (hasAssignedLocalPlayer)
            return;

        var localPlayerPosition = statistics.FindIndex(s => s.PlayerId.IsMe);
        if (localPlayerPosition == -1)
            return;

        var localPlayerData = statistics[localPlayerPosition];
        var localEntry = Entries[localPlayerPosition + 1];
        SetEntryData(localEntry, localPlayerData, localPlayerPosition + 1);
    }
    
    private static void LoadEntries()
    {
        if (_poolee == null)
            return;
        
        Entries.Clear();
        var playerList = _poolee.transform.Find("Center/Players");
        if (playerList == null)
        {
            Debug.LogError("Failed to find player list in leaderboard");
            return;
        }

        for (var i = 0; i < playerList.childCount; i++)
        {
            var child = playerList.GetChild(i);
            if (child == null)
                continue;

            Entries.Add(new LeaderboardPlayerEntry(child.gameObject));
        }
    }
    
    public static void ShowLeaderboard(Vector3 position)
    {
        Spawn(position);
    }
    
    public static void HideLeaderboard()
    {
        if (_poolee == null)
            return;

        _poolee.gameObject.SetActive(false);
    }
}