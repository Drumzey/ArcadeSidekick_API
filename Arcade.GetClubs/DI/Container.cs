using System;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.GetClubs.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IObjectRepository, ObjectRepository>()
                .AddScoped<IClubRepository, ClubRepository>()
                .BuildServiceProvider();
        }
    }
}
