using System;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Arcade.GetScore.Tests.DI
{
    public class Container
    {
        public static IServiceProvider Services(
            Mock<IEnvironmentVariables> mockEnvironmentVariables,
            Mock<IUserRepository> mockUserRepository)
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables>(sp => mockEnvironmentVariables.Object)
                .AddScoped<IUserRepository>(sp => mockUserRepository.Object)
                .BuildServiceProvider();
        }
    }
}
