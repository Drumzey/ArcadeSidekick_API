using System;
using Amazon;
using Amazon.Lambda;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.Authorizer.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()                
                .AddScoped<IAmazonLambda, AmazonLambdaClient>(sp => new AmazonLambdaClient(RegionEndpoint.EUWest2))
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IUserRepository, UserInformationRepository>()
                .BuildServiceProvider();
        }
    }
}
