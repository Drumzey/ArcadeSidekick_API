using System;
using Arcade.Shared;
using Arcade.Shared.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.MyMessages_GET.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IMessageRepository, MessageRepository>()
                .BuildServiceProvider();
        }
    }
}
