using System;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.SaveRating.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IRatingRepository, RatingInformationRepository>()
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IObjectRepository, ObjectRepository>()
                .BuildServiceProvider();
        }
    }
}
