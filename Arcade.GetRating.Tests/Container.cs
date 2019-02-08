using Microsoft.Extensions.DependencyInjection;
using Moq;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using System;

namespace Arcade.GetRating.Tests.DI
{
    public class Container
    {
        public static IServiceProvider Services(
            Mock<IEnvironmentVariables> mockEnvironmentVariables,
            Mock<IRatingRepository> mockRatingRepository)
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables>(sp => mockEnvironmentVariables.Object)
                .AddScoped<IRatingRepository>(sp => mockRatingRepository.Object)                
                .BuildServiceProvider();
        }
    }
}
