using System;
using Arcade.GameDetails;
using Arcade.Shared;
using Arcade.Shared.Misc;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.RatingMigration.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IMiscRepository, MiscRepository>()
                .AddScoped<IGameDetailsRepository, GameDetailsRepository>()
                .BuildServiceProvider();
        }
    }
}
