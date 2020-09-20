using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace HGV.Nullifier.Collection.Models
{
    public partial class MatchReponse
    {
        [JsonProperty("result")]
        public MatchResult Result { get; set; }
    }

    public partial class MatchResult
    {
        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("matches")]
        public List<MatchHistory> Matches { get; set; }
    }

    public class MatchSummary
    {
        [JsonProperty("match_id")]
        public long MatchId { get; set; }

        [JsonProperty("match_seq_num")]
        public long MatchSeqNum { get; set; }

        [JsonProperty("start_time")]
        public long? StartTime { get; set; }

        [JsonProperty("duration")]
        public long? Duration { get; set; }
    }

    public partial class MatchHistory
    {
        [JsonProperty("id")]
        public string Id  => MatchId.ToString();

        [JsonProperty("key")]
        public string Key => Cluster.ToString();

        [JsonProperty("match_id")]
        public long MatchId { get; set; }

        [JsonProperty("match_seq_num")]
        public long MatchSeqNum { get; set; }

        [JsonProperty("cluster")]
        public int Cluster { get; set; }

        [JsonProperty("radiant_win", NullValueHandling = NullValueHandling.Ignore)]
        public bool? RadiantWin { get; set; }

        [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
        public long? Duration { get; set; }

        [JsonProperty("pre_game_duration", NullValueHandling = NullValueHandling.Ignore)]
        public long? PreGameDuration { get; set; }

        [JsonProperty("start_time", NullValueHandling = NullValueHandling.Ignore)]
        public long? StartTime { get; set; }

        [JsonProperty("tower_status_radiant", NullValueHandling = NullValueHandling.Ignore)]
        public long? TowerStatusRadiant { get; set; }

        [JsonProperty("tower_status_dire", NullValueHandling = NullValueHandling.Ignore)]
        public long? TowerStatusDire { get; set; }

        [JsonProperty("barracks_status_radiant", NullValueHandling = NullValueHandling.Ignore)]
        public long? BarracksStatusRadiant { get; set; }

        [JsonProperty("barracks_status_dire", NullValueHandling = NullValueHandling.Ignore)]
        public long? BarracksStatusDire { get; set; }

        [JsonProperty("first_blood_time", NullValueHandling = NullValueHandling.Ignore)]
        public long? FirstBloodTime { get; set; }

        [JsonProperty("lobby_type", NullValueHandling = NullValueHandling.Ignore)]
        public long? LobbyType { get; set; }

        [JsonProperty("human_players", NullValueHandling = NullValueHandling.Ignore)]
        public long? HumanPlayers { get; set; }

        [JsonProperty("leagueid", NullValueHandling = NullValueHandling.Ignore)]
        public long? LeagueId { get; set; }

        [JsonProperty("positive_votes", NullValueHandling = NullValueHandling.Ignore)]
        public long? PositiveVotes { get; set; }

        [JsonProperty("negative_votes", NullValueHandling = NullValueHandling.Ignore)]
        public long? NegativeVotes { get; set; }

        [JsonProperty("game_mode", NullValueHandling = NullValueHandling.Ignore)]
        public long? GameMode { get; set; }

        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public long? Flags { get; set; }

        [JsonProperty("engine", NullValueHandling = NullValueHandling.Ignore)]
        public long? Engine { get; set; }

        [JsonProperty("radiant_score", NullValueHandling = NullValueHandling.Ignore)]
        public int? RadiantScore { get; set; }

        [JsonProperty("dire_score", NullValueHandling = NullValueHandling.Ignore)]
        public int? DireScore { get; set; }

        [JsonProperty("players")]
        public List<PlayerHistory> Players { get; set; }
    }

    public partial class PlayerHistory
    {
        [JsonProperty("account_id", NullValueHandling = NullValueHandling.Ignore)]
        public long? AccountId { get; set; }

        [JsonProperty("player_slot", NullValueHandling = NullValueHandling.Ignore)]
        public int? PlayerSlot { get; set; }

        [JsonProperty("hero_id", NullValueHandling = NullValueHandling.Ignore)]
        public int? HeroId { get; set; }

        [JsonProperty("item_0", NullValueHandling = NullValueHandling.Ignore)]
        public int? Item0 { get; set; }

        [JsonProperty("item_1", NullValueHandling = NullValueHandling.Ignore)]
        public int? Item1 { get; set; }

        [JsonProperty("item_2", NullValueHandling = NullValueHandling.Ignore)]
        public int? Item2 { get; set; }

        [JsonProperty("item_3", NullValueHandling = NullValueHandling.Ignore)]
        public int? Item3 { get; set; }

        [JsonProperty("item_4", NullValueHandling = NullValueHandling.Ignore)]
        public int? Item4 { get; set; }

        [JsonProperty("item_5", NullValueHandling = NullValueHandling.Ignore)]
        public int? Item5 { get; set; }

        [JsonProperty("backpack_0", NullValueHandling = NullValueHandling.Ignore)]
        public int? Backpack0 { get; set; }

        [JsonProperty("backpack_1", NullValueHandling = NullValueHandling.Ignore)]
        public int? Backpack1 { get; set; }

        [JsonProperty("backpack_2", NullValueHandling = NullValueHandling.Ignore)]
        public int? Backpack2 { get; set; }

        [JsonProperty("item_neutral", NullValueHandling = NullValueHandling.Ignore)]
        public int? ItemNeutral { get; set; }

        [JsonProperty("kills", NullValueHandling = NullValueHandling.Ignore)]
        public int? Kills { get; set; }

        [JsonProperty("deaths", NullValueHandling = NullValueHandling.Ignore)]
        public int? Deaths { get; set; }

        [JsonProperty("assists", NullValueHandling = NullValueHandling.Ignore)]
        public int? Assists { get; set; }

        [JsonProperty("leaver_status", NullValueHandling = NullValueHandling.Ignore)]
        public int? LeaverStatus { get; set; }

        [JsonProperty("last_hits", NullValueHandling = NullValueHandling.Ignore)]
        public int? LastHits { get; set; }

        [JsonProperty("denies", NullValueHandling = NullValueHandling.Ignore)]
        public int? Denies { get; set; }

        [JsonProperty("gold_per_min", NullValueHandling = NullValueHandling.Ignore)]
        public double? GoldPerMin { get; set; }

        [JsonProperty("xp_per_min", NullValueHandling = NullValueHandling.Ignore)]
        public double? XpPerMin { get; set; }

        [JsonProperty("level", NullValueHandling = NullValueHandling.Ignore)]
        public int? Level { get; set; }

        [JsonProperty("hero_damage", NullValueHandling = NullValueHandling.Ignore)]
        public int? HeroDamage { get; set; }

        [JsonProperty("tower_damage", NullValueHandling = NullValueHandling.Ignore)]
        public int? TowerDamage { get; set; }

        [JsonProperty("hero_healing", NullValueHandling = NullValueHandling.Ignore)]
        public int? HeroHealing { get; set; }

        [JsonProperty("gold", NullValueHandling = NullValueHandling.Ignore)]
        public int? Gold { get; set; }

        [JsonProperty("gold_spent", NullValueHandling = NullValueHandling.Ignore)]
        public int? GoldSpent { get; set; }

        [JsonProperty("scaled_hero_damage", NullValueHandling = NullValueHandling.Ignore)]
        public int? ScaledHeroDamage { get; set; }

        [JsonProperty("scaled_tower_damage", NullValueHandling = NullValueHandling.Ignore)]
        public int? ScaledTowerDamage { get; set; }

        [JsonProperty("scaled_hero_healing", NullValueHandling = NullValueHandling.Ignore)]
        public int? ScaledHeroHealing { get; set; }

        [JsonProperty("ability_upgrades", NullValueHandling = NullValueHandling.Ignore)]
        public List<AbilityUpgrade> AbilityUpgrades { get; set; }
    }

    public partial class AbilityUpgrade
    {
        [JsonProperty("ability")]
        public int Ability { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }
    }
}
