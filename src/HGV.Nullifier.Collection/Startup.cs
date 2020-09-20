using System;
using System.Collections.Generic;
using System.Net.Http;
using AutoMapper;
using HGV.Nullifier.Collection.Handlers;
using HGV.Nullifier.Collection.Profiles;
using HGV.Nullifier.Collection.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

[assembly: FunctionsStartup(typeof(HGV.Nullifier.Collection.Startup))]

namespace HGV.Nullifier.Collection
{
    public class Startup: FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //builder.Services.AddHttpClient();

            var urlTemplate = "http://api.steampowered.com/IDOTA2Match_570/{0}/v0001/";

            builder.Services.AddTransient<ApiKeyHandler>();
            builder.Services
                .AddHttpClient("account_history")
                .ConfigureHttpClient(c => { 
                    c.BaseAddress = new Uri(string.Format(urlTemplate, "GetMatchHistory"));
                })
                .AddHttpMessageHandler<ApiKeyHandler>();

            builder.Services
                .AddHttpClient("match_history")
                .ConfigureHttpClient(c => { 
                    c.BaseAddress = new Uri(string.Format(urlTemplate, "GetMatchHistory"));
                })
                .AddHttpMessageHandler<ApiKeyHandler>();

            builder.Services.AddSingleton<IObjectiveService, ObjectiveService>();
            builder.Services.AddSingleton<ITeamService, TeamService>();
            builder.Services.AddSingleton<ILocationService, LocationService>();
            
            builder.Services.AddTransient<ICollectionService, CollectionService>();

            builder.Services.AddAutoMapper(typeof(MatchProfile));
        }
    }
}
