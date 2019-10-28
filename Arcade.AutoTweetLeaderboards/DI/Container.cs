using System;
using Arcade.Shared;
using Arcade.Shared.Misc;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.AutoTweetLeaderboards.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IObjectRepository, ObjectRepository>()
                .AddScoped<IMiscRepository, MiscRepository>()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .BuildServiceProvider();
        }
    }
}
