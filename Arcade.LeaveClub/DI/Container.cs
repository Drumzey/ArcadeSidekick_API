using System;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.LeaveClub.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IClubRepository, ClubRepository>()
                .AddScoped<IUserRepository, UserRepository>()
                .BuildServiceProvider();
        }
    }
}
