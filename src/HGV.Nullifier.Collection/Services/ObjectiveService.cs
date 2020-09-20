using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HGV.Nullifier.Collection.Services
{
    [Flags]
    public enum TowerStatus : int
    {
        Tier1Bottom = 1,
        Tier2Bottom = 2,
        Tier3Bottom = 4,
        Tier1Middle = 8,
        Tier2Middle = 16,
        Tier3Middle = 32,
        Tier1Top = 64,
        Tier2Top = 128,
        Tier3Top = 256,
        AncientBottom = 512,
        AncientTop = 1024
    }

    [Flags]
    public enum BarracksStatus : int
    {
        MeleeBottom = 1,
        RangedBottom = 2,
        MeleeMiddle = 4,
        RangedMiddle = 8,
        MeleeTop = 16,
        RangedTop = 32
    }

    public interface IObjectiveService
    {
        double CalculateLost(long? towerStatus, long? barracksStatus);
    }

    public class ObjectiveService : IObjectiveService
    {
        private readonly List<TowerStatus> Towers;
        private readonly List<BarracksStatus> Barracks;
        private readonly int TotalObjectives;

        public ObjectiveService()
        {
            this.Towers = new List<TowerStatus>()
            {
                TowerStatus.Tier1Bottom,
                TowerStatus.Tier1Top,
                TowerStatus.Tier1Middle,
                TowerStatus.Tier2Bottom,
                TowerStatus.Tier2Top,
                TowerStatus.Tier2Middle,
                TowerStatus.Tier3Bottom,
                TowerStatus.Tier3Top,
                TowerStatus.Tier3Middle,
                TowerStatus.AncientTop,
                TowerStatus.AncientBottom,
            };
            this.Barracks = new List<BarracksStatus>()
            {
                BarracksStatus.RangedBottom,
                BarracksStatus.RangedMiddle,
                BarracksStatus.RangedTop,
                BarracksStatus.MeleeBottom,
                BarracksStatus.MeleeMiddle,
                BarracksStatus.MeleeTop,
            };
            this.TotalObjectives = this.Towers.Count + this.Barracks.Count;
        }

        public double CalculateLost(long? towerStatus, long? barracksStatus)
        {
            if (towerStatus.HasValue == false || barracksStatus.HasValue == false)
                return 0;

            var t = (TowerStatus)towerStatus;
            var b = (BarracksStatus) barracksStatus;

            var tower = this.Towers.Select(_ => t.HasFlag(_) ? 0.0 : 1.0).Sum();
            var barracks = this.Barracks.Select(_ => b.HasFlag(_) ? 0.0 : 1.0).Sum();
            var result = (tower + barracks) / this.TotalObjectives;
            return result;
        }
    }
}