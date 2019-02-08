using Microsoft.Extensions.DependencyInjection;
using Moq;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using System;

namespace Arcade.CreateUser.Tests.DI
{
    public class Container
    {
        public static IServiceProvider Services(
            Mock<IEnvironmentVariables> mockEnvironmentVariables,
            Mock<IUserRepository> mockRatingRepository,
            Mock<IEmail> mockEmail)
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables>(sp => mockEnvironmentVariables.Object)
                .AddScoped<IUserRepository>(sp => mockRatingRepository.Object)
                .AddScoped<IEmail>(sp => mockEmail.Object)
                .BuildServiceProvider();
        }
    }
}
