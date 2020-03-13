using Arcade.Shared;
using Arcade.Shared.Locations;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Arcade.Locations.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<ILocationRepository, LocationRepository>()
                .BuildServiceProvider();
        }
    }
}
