using Arcade.Shared;
using Arcade.Shared.Repositories;
using System;
using Xunit;

namespace Arcade.GetScore.Tests.DI
{
    public class ContainerTests
    {
        [Theory]
        [InlineData(typeof(IEnvironmentVariables))]
        [InlineData(typeof(IUserRepository))]        
        public void SharedDI_ReturnsAnInstanceForEachRegisteredType(Type registeredType)
        {
            var services = Arcade.GetScore.DI.Container.Services();
            Assert.NotNull(services.GetService(registeredType));
        }
    }
}
