using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Nullifier.Common.Export
{
    public class HeroAttribute
    {
        public int HeroId { get; set; }
        public int AttributeBaseStrength { get; set; }
        public double RankingBaseStrength { get; set; }
        public double AttributeStrengthGain { get; set; }
        public double RankingStrengthGain { get; set; }
        public int AttributeBaseAgility { get; set; }
        public double RankingBaseAgility { get; set; }
        public double AttributeAgilityGain { get; set; }
        public double RankingAgilityGain { get; set; }
        public int AttributeBaseIntelligence { get; set; }
        public double RankingBaseIntelligence { get; set; }
        public double AttributeIntelligenceGain { get; set; }
        public double RankingIntelligenceGain { get; set; }
        public int StatusHealth { get; set; }
        public double RankingHealth { get; set; }
        public double StatusHealthRegen { get; set; }
        public double RankingHealthRegen { get; set; }
        public int StatusMana { get; set; }
        public double RankingMana { get; set; }
        public double StatusManaRegen { get; set; }
        public double RankingManaRegen { get; set; }
        public int AttackRange { get; set; }
        public double RankingAttackRange { get; set; }
        public int AttackDamageMin { get; set; }
        public int AttackDamageMax { get; set; }
        public double RankingAttackDamage { get; set; }
        public int MovementSpeed { get; set; }
        public double RankingMovementSpeed { get; set; }
        public double MovementTurnRate { get; set; }
        public double RankingMovementTurnRate { get; set; }
        public int VisionDaytimeRange { get; set; }
        public double RankingVisionDaytimeRange { get; set; }
        public int VisionNighttimeRange { get; set; }
        public double RankingVisionNighttimeRange { get; set; }
    }
}
