using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xunit;

namespace Arcade.CreateUser.Tests
{
    public class CreateUserHandlerTests
    {
        private const string userInput = "{\"Username\":\"Drumzey\",\"EmailAddress\":\"Drumzey@test.com\"}";

        [Fact]
        public void CreateUserHandler_WhenCalledWithNewUser_CreatesUser()
        {
            APIGatewayProxyRequest request;
            var headers = new Dictionary<string, string>()
            {
            };
            request = new APIGatewayProxyRequest
            {
                Body = userInput,
            };

            UserInformation info = null;

            var userInfoRepository = new Mock<IUserRepository>();
            userInfoRepository.Setup(x => x.Load(It.IsAny<string>())).Returns(info);

            var email = new Mock<IEmail>();            

            var services = DI.Container.Services(null, userInfoRepository, email);

            var function = new CreateUser(services);
            var context = new TestLambdaContext();
            var result = function.CreateUserHandler(request, context);

            var OKResponse = result.Body;

            Assert.Equal("User record created and secret emailed", OKResponse);

            userInfoRepository.Verify(k => k.Load(It.IsAny<String>()), Times.Once());
            userInfoRepository.Verify(k => k.Save(It.IsAny<UserInformation>()), Times.Once());
            email.Verify(k => k.EmailSecret(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>()), Times.Once());
        }

        [Fact]
        public void CreateUserHandler_WhenCalledWithExistingUser_DoesNotCreateUser()
        {
            APIGatewayProxyRequest request;
            var headers = new Dictionary<string, string>()
            {
            };
            request = new APIGatewayProxyRequest
            {
                Body = userInput,
            };

            UserInformation info = new UserInformation
            {
                Username = "Drumzey",
            };

            var userInfoRepository = new Mock<IUserRepository>();
            userInfoRepository.Setup(x => x.Load(It.IsAny<string>())).Returns(info);

            var email = new Mock<IEmail>();

            var services = DI.Container.Services(null, userInfoRepository, email);

            var function = new CreateUser(services);
            var context = new TestLambdaContext();
            var result = function.CreateUserHandler(request, context);

            var failureResponse = result.Body;

            Assert.Equal("Username already exists", failureResponse);

            userInfoRepository.Verify(k => k.Load(It.IsAny<String>()), Times.Once());
            userInfoRepository.Verify(k => k.Save(It.IsAny<UserInformation>()), Times.Never);
            email.Verify(k => k.EmailSecret(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>()), Times.Never);
        }
    }
}
