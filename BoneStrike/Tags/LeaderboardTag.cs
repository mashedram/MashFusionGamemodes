using BoneStrike.Teams;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using Il2CppTMPro;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.RPC;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Player.Actions;
using MashGamemodeLibrary.Player.Team;
using UnityEngine;
using UnityEngine.UI;

namespace BoneStrike.Tags;

internal class LeaderboardPlayerEntry
{
    public const string Barcode = "Mash.BoneStrike.Spawnable.Leaderboard";

    public GameObject Root;
    public Image Background;
    public TextMeshPro PositionText;
    public TextMeshPro NameText;

    public TextMeshPro KillsText;
    public TextMeshPro DeathsText;
    public TextMeshPro AssistsText;
    public TextMeshPro DefusesText;
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
        DefusesText = root.transform.Find("Content/Stats/Defuses").GetComponent<TextMeshPro>();
        ScoreText = root.transform.Find("Content/Stats/Score").GetComponent<TextMeshPro>();
    }
}

public class LeaderboardPlayerData
{
    public PlayerID PlayerId { get; init; }
    public int Kills { get; init; }
    public int Deaths { get; init; }
    public int Assists { get; init; }
    public int Defuses { get; init; }
    public int Score { get; init; }

    public LeaderboardPlayerData(PlayerStatistics statistics)
    {
        PlayerId = PlayerIDManager.GetPlayerID(statistics.PlayerID);
        Kills = statistics.GetValue(PlayerDamageStatistics.Kills);
        Deaths = statistics.GetValue(PlayerDamageStatistics.Deaths);
        Assists = statistics.GetValue(PlayerDamageStatistics.Assists);
        Defuses = statistics.GetValue(BonestrikeStatisticsKeys.Defusals);

        Score = (Kills + Assists / 2 + Defuses * 2 - Deaths) * 125;
    }
}

public class LeaderboardTag : IComponent, IComponentReady
{
    private static NetworkEntity? _networkEntity;
    private readonly List<LeaderboardPlayerEntry> _entries = new();

    private static void SetEntryData(LeaderboardPlayerEntry entry, LeaderboardPlayerData data, int position)
    {
        entry.PositionText.text = position.ToString();
        var nickname = data.PlayerId.Metadata.Nickname.GetValueOrEmpty();
        var name = string.IsNullOrEmpty(nickname) ? data.PlayerId.Metadata.Username.GetValueOrEmpty() : nickname;
        entry.NameText.text = name;
        entry.KillsText.text = data.Kills.ToString();
        entry.DeathsText.text = data.Deaths.ToString();
        entry.AssistsText.text = data.Assists.ToString();
        entry.DefusesText.text = data.Defuses.ToString();
        entry.ScoreText.text = data.Score.ToString();

        entry.Background.color = data.PlayerId.IsTeam<TerroristTeam>() ? new Color(1f, 0.2f, 0.2f) : new Color(0.2f, 0.2f, 1f);
    }

    public void SetContent()
    {
        var statistics = GlobalStatisticsCollector.Statistics
            .Select(v => new LeaderboardPlayerData(v))
            .Where(v => v.PlayerId.IsValid)
            .ToList();
        statistics.Sort((a, b) => b.Score.CompareTo(a.Score));

        var hasAssignedLocalPlayer = false;
        // We need to skip the header, thus the -1
        for (var i = 0; i < _entries.Count - 1; i++)
        {
            var entry = _entries[i + 1];

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
        var localEntry = _entries[localPlayerPosition + 1];
        SetEntryData(localEntry, localPlayerData, localPlayerPosition + 1);
    }

    public void LoadEntries(MarrowEntity marrowEntity)
    {
        var playerList = marrowEntity.transform.Find("Center/Players");
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

            _entries.Add(new LeaderboardPlayerEntry(child.gameObject));
        }
    }

    public void OnReady(NetworkEntity networkEntity, MarrowEntity marrowEntity)
    {
        Executor.RunIfHost(() =>
        {
            if (_networkEntity != null)
                GameAssetSpawner.Despawn(_networkEntity);
            _networkEntity = networkEntity;
        });

        LoadEntries(marrowEntity);
        SetContent();
    }

    public static void Spawn(Vector3 position)
    {
        GameAssetSpawner.SpawnNetworkAsset(LeaderboardPlayerEntry.Barcode, position, new LeaderboardTag());
    }

    public static void Despawn()
    {
        if (_networkEntity == null)
            return;

        GameAssetSpawner.Despawn(_networkEntity);
        _networkEntity = null;
    }
}