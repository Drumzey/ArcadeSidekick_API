﻿using System;
using Arcade.Shared;
using Arcade.Shared.Misc;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.GetUsers.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IMiscRepository, MiscRepository>()
                .BuildServiceProvider();
        }
    }
}
