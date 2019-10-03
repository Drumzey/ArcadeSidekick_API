using System;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.TopFifty.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IObjectRepository, ObjectRepository>()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .BuildServiceProvider();
        }
    }
}
