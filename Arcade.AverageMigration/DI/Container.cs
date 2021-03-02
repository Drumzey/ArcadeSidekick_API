using System;
using Microsoft.Extensions.DependencyInjection;
using Arcade.Shared;
using Arcade.Shared.Misc;
using Arcade.GameDetails;

namespace Arcade.AverageMigration.DI
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
