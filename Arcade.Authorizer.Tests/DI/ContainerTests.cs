﻿using System;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Xunit;

namespace Arcade.Authorizer.Tests.DI
{
    public class ContainerTests
    {
        [Theory]
        [InlineData(typeof(IEnvironmentVariables))]
        [InlineData(typeof(IUserRepository))]
        public void SharedDI_ReturnsAnInstanceForEachRegisteredType(Type registeredType)
        {
            var services = Arcade.Authorizer.DI.Container.Services();
            Assert.NotNull(services.GetService(registeredType));
        }
    }
}
