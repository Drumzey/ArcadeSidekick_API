using System;
using Arcade.Shared;
using Arcade.Shared.Misc;
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
                .AddScoped<IMiscRepository, MiscRepository>()
                .AddScoped<IClubRepository, ClubRepository>()
                .BuildServiceProvider();
        }
    }
}
