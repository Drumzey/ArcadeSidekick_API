using System;
using Arcade.Shared;
using Arcade.Shared.Messages;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.SaveScore.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IObjectRepository, ObjectRepository>()
                .AddScoped<IMessageRepository, MessageRepository>()
                .BuildServiceProvider();
        }
    }
}
