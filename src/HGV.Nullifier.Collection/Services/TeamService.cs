using System;
using System.Collections.Generic;
using System.Text;
using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HGV.Nullifier.Collection.Services
{

    public interface ITeamService
    {
        int GetTeam(int? slot);
        int GetDraftOrder(int? slot);
        bool Victor(int team, bool? winner);
        int[] GetStandings(bool? winner);
    }

    public static class TeamNames
    {
        public const int Unknown = 0;
        public const int Radiant = 1;
        public const int Dire = 2;
    }

    public class TeamService : ITeamService
    {
        private readonly List<int> Radiant;
        private readonly List<int> Dire;
        private readonly Dictionary<int, int> Order;

        public TeamService()
        {
            this.Radiant = new List<int>() { 0, 1, 2, 3, 4 };
            this.Dire = new List<int>() { 128, 129, 130, 131, 132 };
            this.Order = new Dictionary<int, int>() { {0,1}, {128,2}, {1,3}, {129,4}, {2,5}, {130,6}, {3,7}, {131,8}, {4,9}, {132,10} };
        }

        public int GetTeam(int? slot)
        {
            if (this.Radiant.Contains(slot.Value))
                return TeamNames.Radiant;
            else if (this.Dire.Contains(slot.Value))
                return TeamNames.Dire;
            else
                return TeamNames.Unknown;
        }

        public int GetDraftOrder(int? slot)
        {
            if (!slot.HasValue)
                return 0;
            else 
                return this.Order[slot.Value];
        }

        public bool Victor(int team, bool? winner)
        { 
            return ((team == TeamNames.Radiant && winner == true) || (team == TeamNames.Dire && winner == false)) ? true : false;
        }

        public int[] GetStandings(bool? winner)
        { 
            return winner.GetValueOrDefault() ? new int[] { 1,2 } : new int[] { 2,1 };
        }
    }
}
