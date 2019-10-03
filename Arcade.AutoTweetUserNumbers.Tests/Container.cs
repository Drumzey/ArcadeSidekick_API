using System;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Arcade.AutoTweetUserNumbers.Tests.DI
{
    public class Container
    {
        public static IServiceProvider Services(
            Mock<IEnvironmentVariables> mockEnvironmentVariables,
            Mock<IObjectRepository> mockObjectRepository)
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables>(sp => mockEnvironmentVariables.Object)
                .AddScoped<IObjectRepository>(sp => mockObjectRepository.Object)
                .BuildServiceProvider();
        }
    }
}
