﻿using System;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.Challenge_GET.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IChallengeRepository, ChallengeRepository>()
                .BuildServiceProvider();
        }
    }
}
