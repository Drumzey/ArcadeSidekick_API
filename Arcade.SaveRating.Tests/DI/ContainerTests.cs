using Arcade.Shared;
using Arcade.Shared.Repositories;
using System;
using Xunit;

namespace Arcade.SaveRating.Tests.DI
{
    public class ContainerTests
    {
        [Theory]
        [InlineData(typeof(IEnvironmentVariables))]
        [InlineData(typeof(IRatingRepository))]        
        public void SharedDI_ReturnsAnInstanceForEachRegisteredType(Type registeredType)
        {
            var services = Arcade.SaveRating.DI.Container.Services();
            Assert.NotNull(services.GetService(registeredType));
        }
    }
}
