using System;
using Arcade.Shared;
using Arcade.Shared.Misc;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.TopFifty.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IMiscRepository, MiscRepository>()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .BuildServiceProvider();
        }
    }
}
