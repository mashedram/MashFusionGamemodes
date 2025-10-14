using System.Reflection;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Player;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Variable.Impl;
using MashGamemodeLibrary.Registry;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Player.Team;

public static class TeamManager
{
    public static readonly Registry<Team> Registry = new();
    private static readonly HashSet<Team> EnabledTeams = new();
    private static readonly IDToHashSyncedDictionary AssignedTeams = new("sync.AssignedTeams");

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
        if (!Registry.TryGet(id, out var team))
        {
            MelonLogger.Error($"Failed to find registered team with name: {typeof(T).Name}");
            return;
        }
        EnabledTeams.Add(team);
    }

    public static void Disable()
    {
        EnabledTeams.Clear();
        Executor.RunIfHost(() =>
        {
            AssignedTeams.Clear();
        });
    }

    public static void Assign<T>(this PlayerID playerID) where T : Team
    {
        Executor.RunIfHost(() =>
        {
            var id = GetTeamID<T>();
            AssignedTeams[playerID] = id;
        }, "Assigning teams");
    }

    public static void AssignAll()
    {
        Executor.RunIfHost(() =>
        {
            var teamIndex = 0;
            var ids = EnabledTeams.Select(s => GetTeamID(s.GetType())).ToList();
            foreach (var networkPlayer in NetworkPlayer.Players)
            {
                var id = ids[teamIndex];
                AssignedTeams[networkPlayer.PlayerID] = id;
                
                teamIndex = (teamIndex + 1) % EnabledTeams.Count;
            }
        }, "Assigning All Player Teams");
    }

    public static void AssignToSmallest(this PlayerID playerID)
    {
        Executor.RunIfHost(() =>
        {
            var team = AssignedTeams.Values.GroupBy(g => g).MinBy(g => g.Count())?.Key;
            if (!team.HasValue) return;

            AssignedTeams[playerID] = team.Value;
        }, "Assigning Player To Smallest Team");
    }
}