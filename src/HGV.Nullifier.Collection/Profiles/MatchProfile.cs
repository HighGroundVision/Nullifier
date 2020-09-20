using AutoMapper;
using HGV.Nullifier.Collection.Models;
using HGV.Nullifier.Collection.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HGV.Nullifier.Collection.Profiles
{
    // IValueResolver<MatchHistory, MatchDetails, MatchIdentity>

    public class MatchConverter: ITypeConverter<MatchHistory, MatchDetails>
    {
        private const long ANONYMOUS_ACCOUNT_ID = 4294967295;

        private readonly IObjectiveService ObjectiveService;
        private readonly ITeamService TeamService;
        private readonly ILocationService LocationService;

        public MatchConverter(IObjectiveService objectiveService, ITeamService teamService, ILocationService locationService) 
        {
            this.ObjectiveService = objectiveService ?? throw new ArgumentNullException(nameof(objectiveService));
            this.TeamService = teamService ?? throw new ArgumentNullException(nameof(teamService));
            this.LocationService = locationService ?? throw new ArgumentNullException(nameof(teamService));
        }

        public MatchDetails Convert(MatchHistory source, MatchDetails destination, ResolutionContext context)
        {
            var match = new MatchDetails();

            match.MatchId = source.MatchId;
            match.MatchSeqNum = source.MatchSeqNum;
            match.LeagueId = source.LeagueId.GetValueOrDefault();

            match.Timestamp = source.StartTime.GetValueOrDefault();
            match.Length =  source.Duration.GetValueOrDefault();
            match.Start = DateTimeOffset.FromUnixTimeSeconds(match.Timestamp).DateTime;
            match.Duration = DateTimeOffset.FromUnixTimeSeconds(match.Length).TimeOfDay;
            match.Day = match.Start.Day;
            match.Hour = match.Start.Hour;

            match.Cluster = source.Cluster;
            match.Region = this.LocationService.GetRegion(match.Cluster);
            match.Area = this.LocationService.GetArea(match.Region);
            
            var winner = source.RadiantWin.GetValueOrDefault();
            var rl = this.ObjectiveService.CalculateLost(source.TowerStatusRadiant, source.BarracksStatusRadiant);
            var dl = this.ObjectiveService.CalculateLost(source.TowerStatusDire, source.BarracksStatusDire);
            var rs = source.RadiantScore.GetValueOrDefault();
            var ds = source.DireScore.GetValueOrDefault();

            var collection = source.Players
                .OrderByDescending(_ => _.Kills)
                .ThenBy(_ => _.Deaths)
                .ThenByDescending(_ => _.Assists)
                .Select((_,i) => new { Player = _, Standing = i + 1 })
                .ToList();

            foreach (var item in collection)
            {
                var p = item.Player;
                var standing = item.Standing;

                var player = new MatchPlayer();
                
                player.PlayerSlot = p.PlayerSlot.GetValueOrDefault();
                player.Team = this.TeamService.GetTeam(player.PlayerSlot);
                player.DraftOrder = this.TeamService.GetDraftOrder(player.PlayerSlot);
                player.Standing = standing;
                
                player.Victory = this.TeamService.Victor(player.Team, winner);
                player.Score = (player.Team == TeamNames.Radiant) ? rs : ds;
                player.ObjectivesLost = (player.Team == TeamNames.Radiant) ? rl : dl;
                player.ObjectivesDestroyed = (player.Team == TeamNames.Radiant) ? dl : rl;

                player.AccountId = p.AccountId.GetValueOrDefault();
                player.Anonymous = player.AccountId == ANONYMOUS_ACCOUNT_ID;
                player.HeroId = p.HeroId.GetValueOrDefault();
                player.Level = p.Level.GetValueOrDefault();
                player.Kills = p.Kills.GetValueOrDefault();
                player.Deaths = p.Deaths.GetValueOrDefault();
                player.Assists = p.Assists.GetValueOrDefault();

                player.Items = p.GetItems();
                player.Abilities = p.GetAbilities();
                player.Abandoned = p.LeaverStatus.GetValueOrDefault() > 2;

                match.Players.Add(player);
            }

            match.Valid.HasInsufficientDuration = match.Duration < TimeSpan.FromMinutes(10);
            match.Valid.HasAbandon = match.Players.Any(_ => _.Abandoned);
            match.Valid.HasOmissions = match.Players.Any(_ => _.Abilities.Count < 4);

            return match;
        }
    }

    public class MatchProfile : Profile
    {
        public MatchProfile()
	    {
            CreateMap<MatchHistory, MatchDetails>().ConvertUsing<MatchConverter>();
        }
    }
}
