using System;
using Arcade.GameDetails;
using Arcade.Shared;
using Arcade.Shared.Leagues;
using Arcade.Shared.Locations;
using Arcade.Shared.Misc;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.GetLeagueScores.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            Console.WriteLine("Getting services");

            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IMiscRepository, MiscRepository>()
                .AddScoped<IClubRepository, ClubRepository>()
                .AddScoped<ILeagueRepository, LeagueRepository>()
                .AddScoped<IGameDetailsRepository, GameDetailsRepository>()
                .AddScoped<ILocationRepository, LocationRepository>()
                .AddScoped<IObjectRepository, ObjectRepository>()
                .BuildServiceProvider();
        }
    }
}
