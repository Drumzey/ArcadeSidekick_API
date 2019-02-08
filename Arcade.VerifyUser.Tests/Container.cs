﻿using Microsoft.Extensions.DependencyInjection;
using Moq;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using System;

namespace Arcade.VerifyUser.Tests.DI
{
    public class Container
    {
        public static IServiceProvider Services(
            Mock<IEnvironmentVariables> mockEnvironmentVariables,
            Mock<IUserRepository> mockRatingRepository)
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables>(sp => mockEnvironmentVariables.Object)
                .AddScoped<IUserRepository>(sp => mockRatingRepository.Object)                
                .BuildServiceProvider();
        }
    }
}
