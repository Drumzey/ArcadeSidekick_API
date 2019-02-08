using System;
using Microsoft.Extensions.DependencyInjection;
using Arcade.Shared;
using Arcade.Shared.Repositories;

namespace Arcade.CreateUser.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()                
                .AddScoped<IUserRepository, UserInformationRepository>()
                .AddScoped<IEmail, Email>()
                .BuildServiceProvider();
        }
    }
}
