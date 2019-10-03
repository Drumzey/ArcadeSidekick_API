using System;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.GetRating.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IRatingRepository, RatingInformationRepository>()
                .BuildServiceProvider();
        }
    }
}
