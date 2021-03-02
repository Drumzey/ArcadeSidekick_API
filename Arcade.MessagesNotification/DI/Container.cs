namespace Arcade.MessagesNotification.DI
{
    public class Container
    {
        public static IServiceProvider Services()
        {
            return new ServiceCollection()
                .AddScoped<IEnvironmentVariables, EnvironmentVariables>()
                .AddScoped<IMessageRepository, MessageRepository>()
                .BuildServiceProvider();
        }
    }
}
