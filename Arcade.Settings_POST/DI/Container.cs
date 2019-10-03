using System;
using Microsoft.Extensions.DependencyInjection;
using Arcade.Shared;
using Arcade.Shared.SettingsByGameName;

namespace Arcade.Settings_POST.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<ISettingsByGameNameRepository, SettingsByGameNameRepository>()
                .BuildServiceProvider();
        }
    }
}
