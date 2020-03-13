using System;
using Arcade.Shared;
using Arcade.Shared.Misc;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Arcade.CreateUser.Tests.DI
{
    public class Container
    {
        public static IServiceProvider Services(
            Mock<IEnvironmentVariables> mockEnvironmentVariables,
            Mock<IUserRepository> mockRatingRepository,
            Mock<IObjectRepository> mockObjectRepository,
            Mock<IMiscRepository> mockMiscRepository,
            Mock<IEmail> mockEmail)
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables>(sp => mockEnvironmentVariables.Object)
                .AddScoped<IUserRepository>(sp => mockRatingRepository.Object)
                .AddScoped<IObjectRepository>(sp => mockObjectRepository.Object)
                .AddScoped<IMiscRepository>(sp => mockMiscRepository.Object)
                .AddScoped<IEmail>(sp => mockEmail.Object)
                .BuildServiceProvider();
        }
    }
}
