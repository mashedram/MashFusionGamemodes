using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.Utilities;
using MashGamemodeLibrary.Data.Random;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Registry.Typed;
using MelonLoader;
using Random = UnityEngine.Random;

namespace MashGamemodeLibrary.Player.Team;

public static class TeamManager
{

    public delegate void OnAssignedTeamHandler(PlayerID playerID, Team team);
    private static readonly HashSet<ulong> EnabledTeams = new();
    public static readonly FactoryTypedRegistry<Team> Registry = new();

    private static readonly SyncedDictionary<byte, Team> AssignedTeams = new("sync.AssignedTeams", new ByteEncoder(),
        new DynamicInstanceEncoder<Team>(Registry));

    static TeamManager()
    {
        AssignedTeams.OnValueAdded += OnAssigned;
        AssignedTeams.OnValueRemoved += OnRemoved;

        MultiplayerHooking.OnPlayerLeft += OnPlayerLeave;
    }

    private static ulong GetTeamID(Type type)
    {
        return Registry.CreateID(type);
    }


    private static ulong GetTeamID<T>() where T : Team
    {
        return GetTeamID(typeof(T));
    }

    // Implementations

    public static void Enable<T>() where T : Team
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

    public static Team? GetLocalTeam()
    {
        var id = PlayerIDManager.LocalSmallID;
        return AssignedTeams.GetValueOrDefault(id);
    }

    public static bool IsTeam<T>(this PlayerID playerID) where T : Team
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

    public static bool IsLocalTeam<T>() where T : Team
    {
        return PlayerIDManager.LocalID.IsTeam<T>();
    }

    public static Team? GetPlayerTeam(PlayerID player)
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

    public static void Assign<T>(this NetworkPlayer player, T team) where T : Team
    {
        Executor.RunIfHost(() =>
        {
            AssignedTeams[player.PlayerID] = team;
        });
    }

    public static void Assign<T>(this PlayerID playerID) where T : Team
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
            var ids = EnabledTeams.Select(id => Registry.Get(id)).OfType<Team>().ToList();
            foreach (var networkPlayer in NetworkPlayer.Players)
            {
                var team = ids[teamIndex];
                AssignedTeams[networkPlayer.PlayerID] = team;

                teamIndex = (teamIndex + 1) % EnabledTeams.Count;
            }
        });
    }

    public static void AssignAll<T>() where T : Team
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

    public static void AssignRandom<T>(IRandomProvider<PlayerID>? provider = null) where T : Team
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

    private static void OnAssigned(byte smallID, Team team)
    {
        if (!NetworkPlayerManager.TryGetPlayer(smallID, out var player))
        {
            MelonLogger.Error($"Failed to assign player of id {smallID} to {team.Name}");
            return;
        }

        team.Assign(player);
    }

    private static void OnRemoved(byte platformId, Team team)
    {
        team.Remove();
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