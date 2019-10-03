using System;
using Microsoft.Extensions.DependencyInjection;
using Arcade.Shared;
using Arcade.Shared.Locations;

namespace Arcade.Location_GET.DI
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
