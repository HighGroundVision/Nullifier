using System.Collections.Generic;
using Newtonsoft.Json;

namespace HGV.Nullifier.Collection.Models.History
{
    public partial class MatchHistory
    {
        [JsonProperty("result")]
        public Result Result { get; set; }
    }

    public partial class Result
    {
        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("matches")]
        public List<Match> Matches { get; set; }
    }

    public partial class Match
    {
        [JsonProperty("match_id")]
        public long MatchId { get; set; }

        [JsonProperty("match_seq_num")]
        public long MatchSeqNum { get; set; }

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

        [JsonProperty("cluster", NullValueHandling = NullValueHandling.Ignore)]
        public long? Cluster { get; set; }

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
        public long? RadiantScore { get; set; }

        [JsonProperty("dire_score", NullValueHandling = NullValueHandling.Ignore)]
        public long? DireScore { get; set; }

        [JsonProperty("players")]
        public List<Player> Players { get; set; }
    }

    public partial class Player
    {
        [JsonProperty("account_id", NullValueHandling = NullValueHandling.Ignore)]
        public long? AccountId { get; set; }

        [JsonProperty("player_slot", NullValueHandling = NullValueHandling.Ignore)]
        public long? PlayerSlot { get; set; }

        [JsonProperty("hero_id", NullValueHandling = NullValueHandling.Ignore)]
        public long? HeroId { get; set; }

        [JsonProperty("item_0", NullValueHandling = NullValueHandling.Ignore)]
        public long? Item0 { get; set; }

        [JsonProperty("item_1", NullValueHandling = NullValueHandling.Ignore)]
        public long? Item1 { get; set; }

        [JsonProperty("item_2", NullValueHandling = NullValueHandling.Ignore)]
        public long? Item2 { get; set; }

        [JsonProperty("item_3", NullValueHandling = NullValueHandling.Ignore)]
        public long? Item3 { get; set; }

        [JsonProperty("item_4", NullValueHandling = NullValueHandling.Ignore)]
        public long? Item4 { get; set; }

        [JsonProperty("item_5", NullValueHandling = NullValueHandling.Ignore)]
        public long? Item5 { get; set; }

        [JsonProperty("backpack_0", NullValueHandling = NullValueHandling.Ignore)]
        public long? Backpack0 { get; set; }

        [JsonProperty("backpack_1", NullValueHandling = NullValueHandling.Ignore)]
        public long? Backpack1 { get; set; }

        [JsonProperty("backpack_2", NullValueHandling = NullValueHandling.Ignore)]
        public long? Backpack2 { get; set; }

        [JsonProperty("item_neutral", NullValueHandling = NullValueHandling.Ignore)]
        public long? ItemNeutral { get; set; }

        [JsonProperty("kills", NullValueHandling = NullValueHandling.Ignore)]
        public long? Kills { get; set; }

        [JsonProperty("deaths", NullValueHandling = NullValueHandling.Ignore)]
        public long? Deaths { get; set; }

        [JsonProperty("assists", NullValueHandling = NullValueHandling.Ignore)]
        public long? Assists { get; set; }

        [JsonProperty("leaver_status", NullValueHandling = NullValueHandling.Ignore)]
        public long? LeaverStatus { get; set; }

        [JsonProperty("last_hits", NullValueHandling = NullValueHandling.Ignore)]
        public long? LastHits { get; set; }

        [JsonProperty("denies", NullValueHandling = NullValueHandling.Ignore)]
        public long? Denies { get; set; }

        [JsonProperty("gold_per_min", NullValueHandling = NullValueHandling.Ignore)]
        public long? GoldPerMin { get; set; }

        [JsonProperty("xp_per_min", NullValueHandling = NullValueHandling.Ignore)]
        public long? XpPerMin { get; set; }

        [JsonProperty("level", NullValueHandling = NullValueHandling.Ignore)]
        public long? Level { get; set; }

        [JsonProperty("hero_damage", NullValueHandling = NullValueHandling.Ignore)]
        public long? HeroDamage { get; set; }

        [JsonProperty("tower_damage", NullValueHandling = NullValueHandling.Ignore)]
        public long? TowerDamage { get; set; }

        [JsonProperty("hero_healing", NullValueHandling = NullValueHandling.Ignore)]
        public long? HeroHealing { get; set; }

        [JsonProperty("gold", NullValueHandling = NullValueHandling.Ignore)]
        public long? Gold { get; set; }

        [JsonProperty("gold_spent", NullValueHandling = NullValueHandling.Ignore)]
        public long? GoldSpent { get; set; }

        [JsonProperty("scaled_hero_damage", NullValueHandling = NullValueHandling.Ignore)]
        public long? ScaledHeroDamage { get; set; }

        [JsonProperty("scaled_tower_damage", NullValueHandling = NullValueHandling.Ignore)]
        public long? ScaledTowerDamage { get; set; }

        [JsonProperty("scaled_hero_healing", NullValueHandling = NullValueHandling.Ignore)]
        public long? ScaledHeroHealing { get; set; }

        [JsonProperty("ability_upgrades", NullValueHandling = NullValueHandling.Ignore)]
        public List<AbilityUpgrade> AbilityUpgrades { get; set; }
    }

    public partial class AbilityUpgrade
    {
        [JsonProperty("ability")]
        public long Ability { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("level")]
        public long Level { get; set; }
    }
}
