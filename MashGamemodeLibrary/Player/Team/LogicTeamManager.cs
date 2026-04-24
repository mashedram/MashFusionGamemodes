using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.Utilities;
using MashGamemodeLibrary.Data.Random;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Data;
using MashGamemodeLibrary.Player.Data.Events.Data;
using MashGamemodeLibrary.Registry.Typed;
using MashGamemodeLibrary.Util;
using MelonLoader;
using Random = UnityEngine.Random;

namespace MashGamemodeLibrary.Player.Team;

[RequireStaticConstructor]
public static class LogicTeamManager
{
    private static readonly HashSet<ulong> EnabledTeams = new();
    public static readonly FactoryTypedRegistry<LogicTeam> Registry = new();

    private static readonly SyncedDictionary<byte, LogicTeam> AssignedTeams = new("sync.AssignedTeams", new ByteEncoder(),
        new DynamicInstanceEncoder<LogicTeam>(Registry));

    static LogicTeamManager()
    {
        AssignedTeams.OnValueAdded += OnAssigned;
        AssignedTeams.OnValueRemoved += OnRemoved;

        MultiplayerHooking.OnPlayerLeft += OnPlayerLeave;
    }

    private static ulong GetTeamID(Type type)
    {
        return Registry.GetOrCreateId(type);
    }


    public static ulong GetTeamID<T>() where T : LogicTeam
    {
        return GetTeamID(typeof(T));
    }

    // Implementations

    public static void Enable<T>() where T : LogicTeam
    {
        var id = GetTeamID<T>();
        EnabledTeams.Add(id);
    }

    public static void Disable()
    {
        EnabledTeams.Clear();
        Executor.RunIfHost(() =>
        {
            AssignedTeams.Clear();
        });
    }

    public static ulong? GetLocalTeamID()
    {
        var id = PlayerIDManager.LocalSmallID;
        if (!AssignedTeams.TryGetValue(id, out var team)) return null;

        return Registry.CreateID(team);
    }

    public static LogicTeam? GetLocalTeam()
    {
        var id = PlayerIDManager.LocalSmallID;
        return AssignedTeams.GetValueOrDefault(id);
    }

    public static bool IsTeam<T>(this PlayerID playerID) where T : LogicTeam
    {
        return AssignedTeams.TryGetValue(playerID, out var team) && team.GetType() == typeof(T);
    }

    public static bool IsTeam(this PlayerID playerID, ulong teamID)
    {
        return AssignedTeams.TryGetValue(playerID, out var team) && Registry.CreateID(team) == teamID;
    }

    public static bool IsEnemy(this PlayerID enemyPlayerID)
    {
        var localTeam = GetLocalTeam();
        if (localTeam == null)
            return false;

        return AssignedTeams.TryGetValue(enemyPlayerID, out var otherTeam) && Registry.GetID(localTeam) != Registry.GetID(otherTeam);
    }

    public static bool IsLocalTeam<T>() where T : LogicTeam
    {
        return PlayerIDManager.LocalID.IsTeam<T>();
    }

    public static ulong? GetPlayerTeamID(PlayerID player)
    {
        if (!AssignedTeams.TryGetValue(player, out var team)) return null;

        return Registry.CreateID(team);
    }

    public static LogicTeam? GetPlayerTeam(PlayerID player)
    {
        return AssignedTeams.GetValueOrDefault(player);
    }

    public static bool IsTeamMember(PlayerID player)
    {
        var localTeam = GetLocalTeam();
        var playerTeam = GetPlayerTeam(player);

        if (localTeam == null || playerTeam == null)
            return false;

        return localTeam.GetType() == playerTeam.GetType();
    }

    public static void Assign<T>(this NetworkPlayer player, T team) where T : LogicTeam
    {
        Executor.RunIfHost(() =>
        {
            AssignedTeams[player.PlayerID] = team;
        });
    }

    public static void Assign<T>(this PlayerID playerID) where T : LogicTeam
    {
        Executor.RunIfHost(() =>
        {
            if (!Registry.TryGet<T>(out var team))
            {
                MelonLogger.Error($"Failed to assign team: {typeof(T).Name}. Team was not registered");
                return;
            }
            AssignedTeams[playerID] = team;
        }, "Assigning teams");
    }

    public static void Assign(this PlayerID playerID, ulong teamID)
    {
        Executor.RunIfHost(() =>
        {
            if (!Registry.TryGet(teamID, out var team))
            {
                MelonLogger.Error($"Failed to assign team: {teamID}. Team was not registered");
                return;
            }
            AssignedTeams[playerID] = team;
        });
    }

    public static void AssignAllRandom()
    {
        Executor.RunIfHost(() =>
        {
            var teamIndex = Random.Range(0, 2);
            var ids = EnabledTeams.Select(id => Registry.Get(id)).OfType<LogicTeam>().ToList();
            foreach (var networkPlayer in NetworkPlayer.Players)
            {
                var team = ids[teamIndex];
                AssignedTeams[networkPlayer.PlayerID] = team;

                teamIndex = (teamIndex + 1) % EnabledTeams.Count;
            }
        });
    }

    public static void AssignAll<T>() where T : LogicTeam
    {
        Executor.RunIfHost(() =>
        {
            if (!Registry.TryGet<T>(out var team))
            {
                MelonLogger.Error($"Failed to assign all teams. {typeof(T).Name} is not registered.");
                return;
            }

            foreach (var networkPlayer in NetworkPlayer.Players) AssignedTeams[networkPlayer.PlayerID] = team;
        });
    }

    public static void AssignToSmallest(this PlayerID playerID)
    {
        Executor.RunIfHost(() =>
        {
            if (EnabledTeams.Count == 0)
                return;

            var teamCounts = new Dictionary<ulong, int>();

            foreach (var enabledTeam in EnabledTeams)
            {
                if (!Registry.TryGetType(enabledTeam, out var type))
                    continue;

                teamCounts[enabledTeam] = AssignedTeams.Count(kv => kv.Value.GetType() == type);
            }

            var teamID = teamCounts.DefaultIfEmpty().MinBy(kv => kv.Value);

            var team = Registry.Get(teamID.Key)!;

            AssignedTeams[playerID] = team;
        });
    }

    public static void AssignRandom<T>(IRandomProvider<PlayerID>? provider = null) where T : LogicTeam
    {
        provider ??= new BasicRandomProvider<PlayerID>(() =>
        {
            return NetworkPlayer.Players.Where(p => p.HasRig).Select(p => p.PlayerID).ToList();
        });

        var playerID = provider.GetRandomValue();
        if (playerID == null)
        {
            MelonLogger.Error($"Failed to select a player to assign to team: {typeof(T).Name}");
            return;
        }

        Assign<T>(playerID);
    }

    // Remote

    private static void OnAssigned(byte smallID, LogicTeam team)
    {
        if (!NetworkPlayerManager.TryGetPlayer(smallID, out var player))
        {
            MelonLogger.Error($"Failed to assign player of id {smallID} to {team.Name}");
            return;
        }

        team.Assign(player);
        PlayerDataManager.CallEventOnAll(new TeamChangedEvent(player.PlayerID, team));

        var phase = GamePhaseManager.ActivePhase;
        if (phase != null)
        {
            team.Try(t => t.OnPhaseChanged(phase));
        }
    }

    private static void OnRemoved(byte smallID, LogicTeam team)
    {
        team.Remove();
        
        if (NetworkPlayerManager.TryGetPlayer(smallID, out var player))
            PlayerDataManager.CallEventOnAll(new TeamChangedEvent(player.PlayerID, null));
        
    }

    public static void OnPhaseChanged(GamePhase activePhase)
    {
        foreach (var team in AssignedTeams.Values)
        {
            team.Try(t => t.OnPhaseChanged(activePhase));
        }
    }

    private static void OnPlayerLeave(PlayerID playerId)
    {
        AssignedTeams.Remove(playerId.SmallID);
    }
}