using System;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Xunit;

namespace Arcade.CreateUser.Tests.DI
{
    public class ContainerTests
    {
        [Theory]
        [InlineData(typeof(IEnvironmentVariables))]
        [InlineData(typeof(IUserRepository))]
        [InlineData(typeof(IEmail))]
        public void SharedDI_ReturnsAnInstanceForEachRegisteredType(Type registeredType)
        {
            var services = Arcade.CreateUser.DI.Container.Services();
            Assert.NotNull(services.GetService(registeredType));
        }
    }
}
