using System;
using Arcade.GameDetails;
using Arcade.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.GetRating.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IGameDetailsRepository, GameDetailsRepository>()
                .BuildServiceProvider();
        }
    }
}
