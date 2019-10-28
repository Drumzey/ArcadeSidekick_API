using System;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.AutoTweetTotals.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .BuildServiceProvider();
        }
    }
}
