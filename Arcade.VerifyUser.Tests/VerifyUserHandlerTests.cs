using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace Arcade.VerifyUser.Tests
{
    public class VerifyUserHandlerTests
    {
        private const string userInput = "{\"Username\":\"Drumzey\",\"EmailAddress\":\"Drumzey@test.com\"}";

        [Fact]
        public void VerifyUserHandler_WhenCalledWithNoUser_ReturnError()
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

            var services = DI.Container.Services(null, userInfoRepository);

            var function = new VerifyUser(services);
            var context = new TestLambdaContext();
            var result = function.VerifyUserHandler(request, context);

            var response = result.Body;

            Assert.Equal("User record not found.", response);
            Assert.Equal(404, result.StatusCode);

            userInfoRepository.Verify(k => k.Load(It.IsAny<String>()), Times.Once());            
        }

        [Fact]
        public void VerifyUserHandler_WhenCalledWithExistingUser_UpdatesUser()
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
                Verified = false,
            };

            var userInfoRepository = new Mock<IUserRepository>();
            userInfoRepository.Setup(x => x.Load(It.IsAny<string>())).Returns(info);

            var services = DI.Container.Services(null, userInfoRepository);

            var function = new VerifyUser(services);
            var context = new TestLambdaContext();
            var result = function.VerifyUserHandler(request, context);

            var OkResponse = result.Body;

            Assert.Equal("User record verified.", OkResponse);
            Assert.True(info.Verified);

            userInfoRepository.Verify(k => k.Load(It.IsAny<String>()), Times.Once());
            userInfoRepository.Verify(k => k.Save(It.IsAny<UserInformation>()), Times.Once());
            
        }
    }
}
