using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zombies.Runtime.GameMeta
{
    public static class Teams
    {
        public static List<Team> teams = new();

        public static bool AreHostile(GameObject a, GameObject b)
        {
            var teamA = GetTeam(a);
            var teamB = GetTeam(b);

            return teamA == null || teamB == null || teamA != teamB;
        }

        public static Team GetTeam(GameObject member)
        {
            foreach (var team in teams)
            {
                if (team.members.Contains(member)) return team;
            }

            return null;
        }
        
        public static Team GetTeam(string name)
        {
            foreach (var team in teams)
            {
                if (string.Equals(team.name.Trim(), name.Trim(), StringComparison.InvariantCultureIgnoreCase)) return team;
            }

            return null;
        }
        
        public static void JoinTeam(string name, GameObject member)
        {
            var team = GetTeam(name);
            if (team != null)
            {
                team.members.Add(member);
            }
            else
            {
                team = new Team(name);
                team.members.Add(member);
                teams.Add(team);
            }
        }
        
        public static void LeaveTeam(string name, GameObject member)
        {
            var team = GetTeam(name);
            team.members.Remove(member);
            if (team.members.Count == 0)
            {
                teams.Remove(team);
            }
        }
        
        public class Team
        {
            public string name;
            public List<GameObject> members = new();

            public Team(string name)
            {
                this.name = name;
            }
        }
    }
}