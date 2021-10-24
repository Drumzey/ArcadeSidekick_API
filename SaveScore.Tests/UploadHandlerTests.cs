using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Arcade.GameDetails.Handlers;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Arcade.SaveScore.Tests
{
    public class UploadHandlerTests
    {
        private const string TwoGames = "{\"UserName\":\"Drumzey\",\"SimpleScores\":{\"Pacman\":\"123456\",\"Bubble Bobble\":\"987654\"},\"Ratings\":{\"Pacman\":9,\"Bubble Bobble\":9},\"CreatedAt\":\"2019-01-22T13:29:08.6505456+00:00\",\"UpdatedAt\":\"2019-01-22T13:29:08.6507032+00:00\"}";
        private const string Complicated = "{\"UserName\":\"TESTUSER1\",\"Ratings\":{\"drumzey_test_game\":\"0\",\"drumzey_test_game_2\":\"0\"},\"SimpleScores\":{\"drumzey_test_game\":\"5\",\"drumzey_test_game_2\":\"4\"},\"DetailedScores\":{\"drumzey_test_game\":[{\"Score\":\"1\",\"Date\":\"10/08/2021\",\"LevelName\":\"FULL GAME\",\"Difficulty\":\"\",\"Lives\":\"\",\"ExtraLives\":\"\",\"Credits\":\"\",\"Location\":\"Home Arcade\",\"Event\":\"N/A___undefined\",\"Clubs\":\"\",\"MameOrPCB\":\"\"},{\"Score\":\"2\",\"Date\":\"10/08/2021\",\"LevelName\":\"The Fortress\",\"Difficulty\":\"Easy\",\"Lives\":\"3\",\"ExtraLives\":\"20K\",\"Credits\":\"1\",\"Location\":\"Home Arcade\",\"Event\":\"N/A___undefined\",\"Clubs\":\"\",\"MameOrPCB\":\"EMULATED\"},{\"Score\":\"3\",\"Date\":\"10/08/2021\",\"LevelName\":\"FULL GAME\",\"Difficulty\":\"\",\"Lives\":\"\",\"ExtraLives\":\"\",\"Credits\":\"\",\"Location\":\"Arcade Club(Bury)\",\"Event\":\"N/A___undefined\",\"Clubs\":\"\",\"MameOrPCB\":\"\"},{\"Score\":\"4\",\"Date\":\"10/08/2021\",\"LevelName\":\"The Fortress\",\"Difficulty\":\"\",\"Lives\":\"\",\"ExtraLives\":\"\",\"Credits\":\"\",\"Location\":\"Arcade Club(Bury)\",\"Event\":\"N/A___undefined\",\"Clubs\":\"\",\"MameOrPCB\":\"\"},{\"Score\":\"5\",\"Date\":\"10/08/2021\",\"LevelName\":\"FULL GAME\",\"Difficulty\":\"\",\"Lives\":\"\",\"ExtraLives\":\"\",\"Credits\":\"\",\"Location\":\"Arcade Club(Bury)\",\"Event\":\"N/A___undefined\",\"Clubs\":\"\",\"MameOrPCB\":\"\"}],\"drumzey_test_game_2\":[{\"Score\":\"1\",\"Date\":\"10/08/2021\",\"LevelName\":\"FULL GAME\",\"Difficulty\":\"\",\"Lives\":\"\",\"ExtraLives\":\"\",\"Credits\":\"\",\"Location\":\"Home Arcade\",\"Event\":\"N/A___undefined\",\"Clubs\":\"\",\"MameOrPCB\":\"\"},{\"Score\":\"2\",\"Date\":\"10/08/2021\",\"LevelName\":\"Chinatown\",\"Difficulty\":\"\",\"Lives\":\"\",\"ExtraLives\":\"\",\"Credits\":\"\",\"Location\":\"Arcade Club(Bury)\",\"Event\":\"N/A___undefined\",\"Clubs\":\"\",\"MameOrPCB\":\"\"},{\"Score\":\"4\",\"Date\":\"10/08/2021\",\"LevelName\":\"FULL GAME\",\"Difficulty\":\"\",\"Lives\":\"\",\"ExtraLives\":\"\",\"Credits\":\"\",\"Location\":\"Arcade Club(Bury)\",\"Event\":\"N/A___undefined\",\"Clubs\":\"\",\"MameOrPCB\":\"\"},{\"Score\":\"3\",\"Date\":\"10/08/2021\",\"LevelName\":\"FULL GAME\",\"Difficulty\":\"\",\"Lives\":\"\",\"ExtraLives\":\"\",\"Credits\":\"\",\"Location\":\"Arcade Club(Bury)\",\"Event\":\"N/A___undefined\",\"Clubs\":\"\",\"MameOrPCB\":\"\"}]}}";

        [Fact]
        public void UploadHandler_WhenCalledWithSimpleInformations_UpdatesUserRecord()
        {
            APIGatewayProxyRequest request;
            var headers = new Dictionary<string, string>()
            {
            };
            request = new APIGatewayProxyRequest
            {
                Body = Complicated,
            };

            UserInformation info = new UserInformation
            {
                Username = "Drumzey",
                Games = new Dictionary<string, string>(),
            };

            var userInfoRepository = new Mock<IUserRepository>();
            userInfoRepository.Setup(x => x.Load(It.IsAny<string>())).Returns(info);

            var services = DI.Container.Services(null, userInfoRepository);

            var function = new UploadAllHandler(services);
            var context = new TestLambdaContext();
            function.UploadData(request.Body);
        }
    }
}
