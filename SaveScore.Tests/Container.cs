using Microsoft.Extensions.DependencyInjection;
using Moq;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.SaveScore.Tests.DI
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
