using System;
using Microsoft.Extensions.DependencyInjection;
using Arcade.Shared;
using Arcade.Shared.Repositories;

namespace Arcade.RatingMigration.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IRatingRepository, RatingInformationRepository>()
                .AddScoped<IObjectRepository, ObjectRepository>()
                .BuildServiceProvider();
        }
    }
}
