using System;
using Arcade.GameDetails;
using Arcade.Shared;
using Arcade.Shared.Misc;
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
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IObjectRepository, ObjectRepository>()
                .AddScoped<IGameDetailsRepository, GameDetailsRepository>()
                .AddScoped<IMiscRepository, MiscRepository>()
                .BuildServiceProvider();
        }
    }
}
