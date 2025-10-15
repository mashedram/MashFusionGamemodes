using System.Reflection;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Player;
using MashGamemodeLibrary.Data.Random;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Variable.Impl;
using MashGamemodeLibrary.Registry;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Player.Team;

public static class TeamManager
{
    private static readonly HashSet<ulong> EnabledTeams = new();
    private static readonly IDToInstanceSyncedDictionary<Team> AssignedTeams = new("sync.AssignedTeams");
    public static IRegistry<Team> Registry => AssignedTeams.Registry;

    public delegate void OnAssignedTeamHandler(PlayerID playerID, Team team);

    public static event OnAssignedTeamHandler? OnAssignedTeam;

    static TeamManager()
    {
        AssignedTeams.OnValueChanged += OnAssigned;
        AssignedTeams.OnValueRemoved += OnRemoved;
    }

    private static ulong GetTeamID(MemberInfo type)
    {
        return type.Name.GetStableHash();
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

        return Registry.GetID(team);
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

    public static bool IsLocalTeam<T>() where T : Team
    {
        return PlayerIDManager.LocalID.IsTeam<T>();
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
        }, "Assigning teams");
    }
    
    public static void RandomAssignAll()
    {
        Executor.RunIfHost(() =>
        {
            var teamIndex = 0;
            var ids = EnabledTeams.Select(id => Registry.Get(id)).OfType<Team>().ToList();
            foreach (var networkPlayer in NetworkPlayer.Players)
            {
                var team = ids[teamIndex];
                AssignedTeams[networkPlayer.PlayerID] = team;
                
                teamIndex = (teamIndex + 1) % EnabledTeams.Count;
            }
        }, "Randomly Assigning All Player Teams");
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
        }, "Assigning All Player Teams");
    }

    public static void AssignToSmallest(this PlayerID playerID)
    {
        Executor.RunIfHost(() =>
        {
            var teamID = AssignedTeams.Values.GroupBy(team => Registry.GetID(team)).MinBy(g => g.Count())?.Key;
            if (!teamID.HasValue) return;
            
            var team = Registry.Get(teamID.Value)!;

            AssignedTeams[playerID] = team;
        }, "Assigning Player To Smallest Team");
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
        var playerID = PlayerIDManager.GetPlayerID(smallID);
        if (playerID == null)
        {
            MelonLogger.Error($"Failed to assign player of id {smallID} to {team.Name}");
            return;
        }
        
        team.OnAssigned(playerID);
        OnAssignedTeam?.Invoke(playerID, team);
    }

    private static void OnRemoved(byte smallID, Team team)
    {
        var playerID = PlayerIDManager.GetPlayerID(smallID);
        if (playerID == null)
        {
            MelonLogger.Error($"Failed to remove player of id {smallID} to {team.Name}");
            return;
        }
        
        team.OnRemoved(playerID);
    }
}