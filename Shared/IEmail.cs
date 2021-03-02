namespace Arcade.Shared
{
    public interface IEmail
    {
        void EmailSecret(string secret, string email, string username, IEnvironmentVariables environment);
    }
}
