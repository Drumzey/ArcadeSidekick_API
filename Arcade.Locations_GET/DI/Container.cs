using System;
using Microsoft.Extensions.DependencyInjection;
using Arcade.Shared;
using Arcade.Shared.Locations;
using Arcade.Shared.ListItems;

namespace Arcade.Locations_GET.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()                
                .AddScoped<ILocationRepository, LocationRepository>()
                .AddScoped<IListItemsRepository, ListItemsRepository>()
                .BuildServiceProvider();
        }
    }
}
