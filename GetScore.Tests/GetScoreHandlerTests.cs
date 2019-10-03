using System;
using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Arcade.GetScore.Tests
{
    public class GetScoreHandlerTests
    {
        private const string UserInput = "{\"Usernames\":\"Drumzey\"}";
        private const string UserInput2Users = "{\"Usernames\":\"DRUMZEY,GUSTREE\"}";

        [Fact]
        public void GetScoreHandler_WhenCalledWithUserNameThatHasNotBeenSeen_ReturnsNull()
        {
            APIGatewayProxyRequest request;
            var headers = new Dictionary<string, string>()
            {
            };
            request = new APIGatewayProxyRequest
            {
                Body = UserInput,
            };

            UserInformation info = null;

            var userInfoRepository = new Mock<IUserRepository>();
            userInfoRepository.Setup(x => x.Load(It.IsAny<string>())).Returns(info);

            var services = DI.Container.Services(null, userInfoRepository);

            var function = new GetScore(services);
            var context = new TestLambdaContext();
            var result = function.GetScoreHandler(request, context);

            var newUserInformation = JsonConvert.DeserializeObject<GetUserInformationResponse>(result.Body);

            Assert.Empty(newUserInformation.Users);

            userInfoRepository.Verify(k => k.Load(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public void GetScoreHandler_WhenCalledWithUserName_ReturnsResult()
        {
            APIGatewayProxyRequest request;
            var headers = new Dictionary<string, string>()
            {
            };
            request = new APIGatewayProxyRequest
            {
                Body = UserInput,
            };

            UserInformation info = new UserInformation
            {
                Username = "Drumzey",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Games = new Dictionary<string, string>
                {
                    { "Pacman", "123456789" },
                    { "Bubble Bobble", "123456789" },
                    { "Amidar", "123456789" },
                },
            };

            var userInfoRepository = new Mock<IUserRepository>();
            userInfoRepository.Setup(x => x.Load("Drumzey")).Returns(info);

            var services = DI.Container.Services(null, userInfoRepository);

            var function = new GetScore(services);
            var context = new TestLambdaContext();
            var result = function.GetScoreHandler(request, context);

            var newUserInformation = JsonConvert.DeserializeObject<GetUserInformationResponse>(result.Body);

            Assert.NotEmpty(newUserInformation.Users[0].Games);

            userInfoRepository.Verify(k => k.Load(It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public void GetScoreHandler_WhenCalledWith2UserNames_ReturnsCorrectResult()
        {
            APIGatewayProxyRequest request;
            var headers = new Dictionary<string, string>()
            {
            };
            request = new APIGatewayProxyRequest
            {
                Body = UserInput2Users,
            };

            UserInformation info = new UserInformation
            {
                Username = "Drumzey",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Games = new Dictionary<string, string>
                {
                    { "Pacman", "123456789" },
                    { "Bubble Bobble", "123456789" },
                    { "Amidar", "123456789" },
                },
            };

            UserInformation info2 = new UserInformation
            {
                Username = "Gustree",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Games = new Dictionary<string, string>
                {
                    { "Pacman", "111111111" },
                    { "Bubble Bobble", "111111111" },
                    { "Amidar", "111111111" },
                },
            };

            var userInfoRepository = new Mock<IUserRepository>();
            userInfoRepository.Setup(x => x.Load("DRUMZEY")).Returns(info);
            userInfoRepository.Setup(x => x.Load("GUSTREE")).Returns(info2);

            var services = DI.Container.Services(null, userInfoRepository);

            var function = new GetScore(services);
            var context = new TestLambdaContext();
            var result = function.GetScoreHandler(request, context);

            var newUserInformation = JsonConvert.DeserializeObject<GetUserInformationResponse>(result.Body);

            Assert.NotEmpty(newUserInformation.Users);
            Assert.Equal(2, newUserInformation.Users.Count);

            userInfoRepository.Verify(k => k.Load(It.IsAny<string>()), Times.Exactly(2));
        }
    }
}
