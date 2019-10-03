using System;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Xunit;

namespace Arcade.GetRating.Tests.DI
{
    public class ContainerTests
    {
        [Theory]
        [InlineData(typeof(IEnvironmentVariables))]
        [InlineData(typeof(IRatingRepository))]
        public void SharedDI_ReturnsAnInstanceForEachRegisteredType(Type registeredType)
        {
            var services = Arcade.GetRating.DI.Container.Services();
            Assert.NotNull(services.GetService(registeredType));
        }
    }
}
