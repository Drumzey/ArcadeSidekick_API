using System;
using Arcade.Shared;
using Arcade.Shared.Misc;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.AutoTweetBestGamers.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IObjectRepository, ObjectRepository>()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IMiscRepository, MiscRepository>()
                .BuildServiceProvider();
        }
    }
}
