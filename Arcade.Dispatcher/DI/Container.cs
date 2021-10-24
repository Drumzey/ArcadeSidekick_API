using System;
using Arcade.GameDetails;
using Arcade.Shared;
using Arcade.Shared.Locations;
using Arcade.Shared.Messages;
using Arcade.Shared.Misc;
using Arcade.Shared.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Arcade.Dispatcher.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IClubRepository, ClubRepository>()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IGameDetailsRepository, GameDetailsRepository>()
                .AddScoped<ILocationRepository, LocationRepository>()
                .AddScoped<IMessageRepository, MessageRepository>()
                .AddScoped<IMiscRepository, MiscRepository>()
                .AddScoped<IObjectRepository, ObjectRepository>()
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IEmail, Email>()
                .BuildServiceProvider();
        }
    }
}
