using System;
using Arcade.Shared;
using Arcade.Shared.Messages;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.EventNews_POST.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IClubRepository, ClubRepository>()
                .AddScoped<IMessageRepository, MessageRepository>()
                .BuildServiceProvider();
        }
    }
}
