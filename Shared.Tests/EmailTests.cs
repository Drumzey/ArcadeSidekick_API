using Arcade.Shared;
using Moq;
using Xunit;

namespace Arcade.CreateUser.Tests
{
    public class EmailTests
    {
        [Fact]
        public void EmailSecret_WhenCalledWithSecretAndEmail_Emails()
        {
            var environ = new Mock<IEnvironmentVariables>();
            environ.Setup(x => x.EmailAddress).Returns("arcadesidekick@outlook.com");
            environ.Setup(x => x.EmailPassword).Returns("Am1darTetr151");

            Email email = new Email();
            email.EmailSecret("1234567", "richard.rumsey@gmail.com", "Drumzey", environ.Object);
        }

        [Fact]
        public void EmailSecret_WhenCalledWithSecretAndEmailWithAWS_Emails()
        {
            Email email = new Email();
            email.EmailSecret("1234567", "richard.rumsey@gmail.com", "Drumzey");
        }
    }
}
