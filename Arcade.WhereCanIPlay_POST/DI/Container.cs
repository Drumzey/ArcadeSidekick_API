using System;
using Microsoft.Extensions.DependencyInjection;
using Arcade.Shared;
using Arcade.Shared.LocationByMachine;

namespace Arcade.WhereCanIPlay_POST.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<ILocationByMachineRepository, LocationByMachineRepository>()
                .BuildServiceProvider();
        }
    }
}
