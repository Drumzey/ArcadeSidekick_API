using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Arcade.SaveScore.Tests
{
    public class SaveHandlerTest
    {
        private const string TwoGames = "{\"Username\":\"Drumzey\",\"Games\":{\"Pacman\":\"123456\",\"Bubble Bobble\":\"987654\"},\"Ratings\":{\"Pacman\":9,\"Bubble Bobble\":9},\"CreatedAt\":\"2019-01-22T13:29:08.6505456+00:00\",\"UpdatedAt\":\"2019-01-22T13:29:08.6507032+00:00\"}";
        private const string TwoGamesUpdated = "{\"Username\":\"Drumzey\",\"Games\":{\"Pacman\":\"999999\",\"Bubble Bobble\":\"987654\"},\"Ratings\":{\"Pacman\":10,\"Bubble Bobble\":10},\"CreatedAt\":\"2019-01-22T13:29:08.6505456+00:00\",\"UpdatedAt\":\"2019-01-22T13:29:08.6507032+00:00\"}";
        private const string TwoNewGames = "{\"Username\":\"Drumzey\",\"Games\":{\"Amidar\":\"111111\",\"Solar Fox\":\"987654\"},\"Ratings\":{\"Amidar\":10,\"Solar Fox\":9},\"CreatedAt\":\"2019-01-22T13:29:08.6505456+00:00\",\"UpdatedAt\":\"2019-01-22T13:29:08.6507032+00:00\"}";

        [Fact]
        public void SaveScoreHandler_WhenCalledWithNewUserInformation_CreatesNewRecord()
        {
            APIGatewayProxyRequest request;
            var headers = new Dictionary<string, string>()
            {
            };
            request = new APIGatewayProxyRequest
            {
                Body = TwoGames,
            };

            UserInformation info = null;

            var userInfoRepository = new Mock<IUserRepository>();
            userInfoRepository.Setup(x => x.Load(It.IsAny<string>())).Returns(info);

            var services = DI.Container.Services(null, userInfoRepository);

            var function = new SaveScore(services);
            var context = new TestLambdaContext();
            var result = function.SaveScoreHandler(request, context);

            var newUserInformation = JsonConvert.DeserializeObject<UserInformation>(result.Body);

            Assert.Equal("Drumzey", newUserInformation.Username);
            Assert.Equal(2, newUserInformation.Games.Count);
            Assert.True(newUserInformation.Games.ContainsKey("Pacman"));
            Assert.True(newUserInformation.Games.ContainsKey("Bubble Bobble"));
            Assert.Equal("123456", newUserInformation.Games["Pacman"]);
            Assert.Equal("987654", newUserInformation.Games["Bubble Bobble"]);

            userInfoRepository.Verify(k => k.Save(It.IsAny<UserInformation>()), Times.Once());
        }

        [Fact]
        public void SaveScoreHandler_WhenCalledWithUpdatedUserInformation_AmendsCurrentRecord()
        {
            APIGatewayProxyRequest request;
            var headers = new Dictionary<string, string>()
            {
            };
            request = new APIGatewayProxyRequest
            {
                Body = TwoGamesUpdated,
            };

            UserInformation info = JsonConvert.DeserializeObject<UserInformation>(TwoGames);

            var userInfoRepository = new Mock<IUserRepository>();
            userInfoRepository.Setup(x => x.Load(It.IsAny<string>())).Returns(info);

            var services = DI.Container.Services(null, userInfoRepository);

            // Invoke the lambda function and confirm the string was upper cased.
            var function = new SaveScore(services);
            var context = new TestLambdaContext();
            var result = function.SaveScoreHandler(request, context);

            var newUserInformation = JsonConvert.DeserializeObject<UserInformation>(result.Body);

            Assert.Equal("Drumzey", newUserInformation.Username);
            Assert.Equal(2, newUserInformation.Games.Count);
            Assert.True(newUserInformation.Games.ContainsKey("Pacman"));
            Assert.True(newUserInformation.Games.ContainsKey("Bubble Bobble"));
            Assert.Equal("999999", newUserInformation.Games["Pacman"]);
            Assert.Equal("987654", newUserInformation.Games["Bubble Bobble"]);

            userInfoRepository.Verify(k => k.Save(It.IsAny<UserInformation>()), Times.Once());
        }

        [Fact]
        public void SaveScoreHandler_WhenCalledWithNewGameInformationButExistingUserInformation_AmendsCurrentRecord()
        {
            APIGatewayProxyRequest request;
            var headers = new Dictionary<string, string>()
            {
            };
            request = new APIGatewayProxyRequest
            {
                Body = TwoNewGames,
            };

            UserInformation info = JsonConvert.DeserializeObject<UserInformation>(TwoGames);

            var userInfoRepository = new Mock<IUserRepository>();
            userInfoRepository.Setup(x => x.Load(It.IsAny<string>())).Returns(info);

            var services = DI.Container.Services(null, userInfoRepository);

            // Invoke the lambda function and confirm the string was upper cased.
            var function = new SaveScore(services);
            var context = new TestLambdaContext();
            var result = function.SaveScoreHandler(request, context);

            var newUserInformation = JsonConvert.DeserializeObject<UserInformation>(result.Body);

            Assert.Equal("Drumzey", newUserInformation.Username);
            Assert.Equal(4, newUserInformation.Games.Count);
            Assert.True(newUserInformation.Games.ContainsKey("Pacman"));
            Assert.True(newUserInformation.Games.ContainsKey("Bubble Bobble"));
            Assert.True(newUserInformation.Games.ContainsKey("Amidar"));
            Assert.True(newUserInformation.Games.ContainsKey("Solar Fox"));
            Assert.Equal("123456", newUserInformation.Games["Pacman"]);
            Assert.Equal("111111", newUserInformation.Games["Amidar"]);
            Assert.Equal("987654", newUserInformation.Games["Solar Fox"]);

            userInfoRepository.Verify(k => k.Save(It.IsAny<UserInformation>()), Times.Once());
        }
    }
}
