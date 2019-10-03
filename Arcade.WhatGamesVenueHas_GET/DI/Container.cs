using System;
using Microsoft.Extensions.DependencyInjection;
using Arcade.Shared;
using Arcade.Shared.MachinesByLocation;

namespace Arcade.WhatGamesVenueHas_GET.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IArcadeMachinesByLocationRepository, ArcadeMachinesByLocationRepository>()
                .BuildServiceProvider();
        }
    }
}
