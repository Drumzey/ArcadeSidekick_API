using Xunit;
using Arcade.Shared;

namespace Arcade.CreateUser.Tests
{
    public class EmailTest
    {
        [Fact]
        public void EmailSecret_WhenCalledWithSecretAndEmail_Emails()
        {
            Email email = new Email();
            email.EmailSecret("1234567", "richard.rumsey@gmail.com");            
        }
    }
}
