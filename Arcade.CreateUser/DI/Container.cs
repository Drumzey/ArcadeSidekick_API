﻿using System;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.CreateUser.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IObjectRepository, ObjectRepository>()
                .AddScoped<IEmail, Email>()
                .BuildServiceProvider();
        }
    }
}
