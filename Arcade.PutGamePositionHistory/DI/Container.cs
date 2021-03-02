using Arcade.Shared;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Arcade.PutGamePositionHistory.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            Console.WriteLine("Getting services");

            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .BuildServiceProvider();
        }
    }
}
