using System;
using HGV.Nullifier.Collection.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(HGV.Nullifier.Collection.Startup))]

namespace HGV.Nullifier.Collection
{
    public class Startup: FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IObjectiveService, ObjectiveService>();
            builder.Services.AddSingleton<ITeamService, TeamService>();
        }
    }
}
