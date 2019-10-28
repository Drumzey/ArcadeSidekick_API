using System;
using Arcade.Shared;
using Arcade.Shared.Locations;
using Arcade.Shared.Misc;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.GameDetails.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IGameDetailsRepository, GameDetailsRepository>()
                .AddScoped<ILocationRepository, LocationRepository>()
                .AddScoped<IObjectRepository, ObjectRepository>()
                .AddScoped<IMiscRepository, MiscRepository>()
                .BuildServiceProvider();
        }
    }
}
