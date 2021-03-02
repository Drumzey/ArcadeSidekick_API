using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Arcade.Shared;
using Arcade.Shared.Misc;
using Arcade.Shared.Repositories;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Arcade.CreateUser.Tests
{
    public class CreateUserHandlerTests
    {
        private const string UserInput = "{\"Username\":\"Drumzey\",\"EmailAddress\":\"Drumzey@test.com\"}";
        private const string UserInputBlankTwitter = "{\"Username\":\"Drumzey\",\"EmailAddress\":\"Drumzey@test.com\",\"TwitterHandle\":\"\"}";
        private const string UserInputTwitter = "{\"Username\":\"Drumzey\",\"EmailAddress\":\"Drumzey@test.com\",\"TwitterHandle\":\"DRUMZEY\"}";

        [Fact]
        public void TEST()
        {
            string jsonInput = "{\"CreatedAt\":\"2019-03-01T14:13:01.082Z\",\"EmailAddress\":\"richard.rumsey@gmail.com\",\"Games\":{\"1942\":\"10000\",\"1943\":\"2\",\"18_wheeler_american_pro_trucker\":\"0\",\"alcon\":\"18910\",\"ali_baba_and_the_40_thieves\":\"0\",\"alien_vs_predator\":\"0\",\"altered_beast\":\"0\",\"amidar\":\"34000\",\"arkanoid\":\"15230\",\"arkanoid_revenge_of_doh\":\"0\",\"art_of_fighting\":\"0\",\"assault\":\"10000\",\"asteroids\":\"7540\",\"asteroids_deluxe\":\"0\",\"astro_blaster\":\"12300\",\"balloon_fight\":\"0\",\"berzerk\":\"2169\",\"black_tiger\":\"0\",\"bomb_jack\":\"9700\",\"bubble_bobble\":\"283940\",\"burger_time\":\"1000\",\"centipede\":\"0\",\"cosmo_gang_the_video\":\"473340\",\"crystal_castles\":\"25400\",\"defender\":\"0\",\"dig_dug\":\"0\",\"dodonpachi_dai-ou-jou\":\"0\",\"dodonpachi_sai-dai-ou-jou_black_label\":\"0\",\"donkey_kong\":\"0\",\"donkey_kong_3\":\"0\",\"donkey_kong_jr\":\"0\",\"dragon_blaze\":\"0\",\"drum_master\":\"0\",\"enduro_racer\":\"406978\",\"frogger\":\"5580\",\"future_tomtom\":\"0\",\"galaga\":\"41810\",\"galaga_-_fast_shoot_hack\":\"0\",\"galaxian\":\"0\",\"gauntlet_ii\":\"12345\",\"ghouls_n_ghosts\":\"0\",\"giga_wing\":\"0\",\"golden_axe\":\"0\",\"gorf\":\"4900\",\"guitar_hero\":\"0\",\"hot_rod\":\"0\",\"house_of_the_dead_2\":\"0\",\"house_of_the_dead_4\":\"0\",\"hunchback\":\"79700\",\"hypersports\":\"34430\",\"ice_cold_beer\":\"2410\",\"ikaruga\":\"0\",\"joust\":\"0\",\"jubeat_qubell\":\"0\",\"karate_champ\":\"13400\",\"ketsui\":\"0\",\"knights_of_the_round\":\"0\",\"kung-fu_master\":\"21470\",\"ladybug\":\"22880\",\"lizard_wizard\":\"96200\",\"mappy\":\"0\",\"mario-bros\":\"0\",\"metal_slug\":\"0\",\"metal_slug_3\":\"0\",\"metal_slug_6\":\"0\",\"moon_patrol\":\"12500\",\"mr_do!\":\"0\",\"ms_pac-man_-_speed_up_hack\":\"0\",\"museca\":\"0\",\"neo_drift_out_new_technology\":\"419990\",\"new_rally_x\":\"71540\",\"new_zealand_story\":\"59800\",\"out_run\":\"3417870\",\"out_run_2\":\"0\",\"pac-land\":\"0\",\"pac_and_paint\":\"1030\",\"pandoras_palace\":\"45550\",\"phoenix\":\"0\",\"point_blank\":\"0\",\"point_blank_2\":\"0\",\"q-bert\":\"0\",\"qix\":\"0\",\"quartet\":\"0\",\"quick_and_crash\":\"5431\",\"rainbow_islands\":\"0\",\"rastan_saga\":\"78500\",\"return_of_the_jedi\":\"0\",\"robocop\":\"28000\",\"robotron_2084\":\"142050\",\"scramble\":\"0\",\"sinistar\":\"0\",\"smash_tv\":\"636140\",\"sunset_riders\":\"0\",\"super_bishi_bashi_champ\":\"71\",\"super_street_fighter_ii_x_grand_master_challenge\":\"0\",\"tempest\":\"0\",\"tetris\":\"0\",\"time_crisis_ii\":\"0\",\"track_and_field\":\"34580\",\"turtles\":\"15540\",\"warlords\":\"0\",\"windjammers\":\"0\",\"zoo_keeper\":\"92130\"},\"Ratings\":{\"1942\":8,\"1943\":8,\"18_wheeler_american_pro_trucker\":4,\"alcon\":6,\"ali_baba_and_the_40_thieves\":0,\"alien_vs_predator\":0,\"altered_beast\":5,\"amidar\":0,\"arkanoid\":6,\"arkanoid_revenge_of_doh\":6,\"art_of_fighting\":0,\"assault\":0,\"asteroids\":7,\"asteroids_deluxe\":6,\"astro_blaster\":4,\"balloon_fight\":6,\"berzerk\":7,\"black_tiger\":0,\"bomb_jack\":6,\"bubble_bobble\":9,\"burger_time\":6,\"centipede\":6,\"cosmo_gang_the_video\":6,\"crystal_castles\":7,\"defender\":8,\"dig_dug\":9,\"dodonpachi_dai-ou-jou\":9,\"dodonpachi_sai-dai-ou-jou_black_label\":9,\"donkey_kong\":8,\"donkey_kong_3\":7,\"donkey_kong_jr\":9,\"dragon_blaze\":8,\"drum_master\":9,\"enduro_racer\":5,\"frogger\":7,\"future_tomtom\":10,\"galaga\":7,\"galaga_-_fast_shoot_hack\":0,\"galaxian\":6,\"gauntlet_ii\":6,\"ghouls_n_ghosts\":8,\"giga_wing\":8,\"golden_axe\":6,\"gorf\":5,\"guitar_hero\":9,\"hot_rod\":9,\"house_of_the_dead_2\":8,\"house_of_the_dead_4\":7,\"hunchback\":9,\"hypersports\":8,\"ice_cold_beer\":8,\"ikaruga\":9,\"joust\":8,\"jubeat_qubell\":8,\"karate_champ\":6,\"ketsui\":8,\"knights_of_the_round\":7,\"kung-fu_master\":4,\"ladybug\":7,\"lizard_wizard\":6,\"mappy\":0,\"mario-bros\":7,\"metal_slug\":9,\"metal_slug_3\":9,\"metal_slug_6\":9,\"moon_patrol\":6,\"mr_do!\":6,\"ms_pac-man_-_speed_up_hack\":8,\"museca\":10,\"neo_drift_out_new_technology\":6,\"new_rally_x\":7,\"new_zealand_story\":8,\"out_run\":8,\"out_run_2\":8,\"pac-land\":6,\"pac_and_paint\":5,\"pandoras_palace\":0,\"phoenix\":6,\"point_blank\":9,\"point_blank_2\":9,\"q-bert\":7,\"qix\":8,\"quartet\":3,\"quick_and_crash\":8,\"rainbow_islands\":9,\"rastan_saga\":6,\"return_of_the_jedi\":7,\"robocop\":5,\"robotron_2084\":9,\"scramble\":7,\"sinistar\":7,\"smash_tv\":7,\"sunset_riders\":6,\"super_bishi_bashi_champ\":10,\"super_street_fighter_ii_x_grand_master_challenge\":7,\"tempest\":9,\"tetris\":9,\"time_crisis_ii\":9,\"track_and_field\":9,\"turtles\":7,\"warlords\":10,\"windjammers\":8,\"zoo_keeper\":8},\"Secret\":\"29aVbiB4Ek4wdWbJtKxvlO7HYrE60tnX5cYF7z0HH90=\",\"UpdatedAt\":\"2019-07-23T07:00:18.925Z\",\"Username\":\"DRUMZEY\",\"Verified\":true}";

            var record = JsonConvert.DeserializeObject<UserInformation>(jsonInput);
        }

        [Fact]
        public void CreateUserHandler_WhenCalledWithNewUser_CreatesUser()
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
            var email = new Mock<IEmail>();
            var enviro = new Mock<IEnvironmentVariables>();
            var objectR = new Mock<IObjectRepository>();
            var misc = new Mock<IMiscRepository>();

            var services = DI.Container.Services(enviro, userInfoRepository, objectR, misc, email);

            var function = new CreateUser(services);
            var context = new TestLambdaContext();
            var result = function.CreateUserHandler(request, context);

            var oKResponse = result.Body;

            Assert.Equal("User record created and secret emailed", oKResponse);

            userInfoRepository.Verify(k => k.Load(It.IsAny<string>()), Times.Once());
            userInfoRepository.Verify(k => k.Save(It.IsAny<UserInformation>()), Times.Once());

            // email.Verify(k => k.EmailSecret(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>()), Times.Once());
        }

        [Fact]
        public void CreateUserHandler_WhenCalledWithNewUserBlankTwitter_CreatesUser()
        {
            APIGatewayProxyRequest request;
            var headers = new Dictionary<string, string>()
            {
            };
            request = new APIGatewayProxyRequest
            {
                Body = UserInputBlankTwitter,
            };

            UserInformation info = null;

            var userInfoRepository = new Mock<IUserRepository>();
            userInfoRepository.Setup(x => x.Load(It.IsAny<string>())).Returns(info);
            var email = new Mock<IEmail>();
            var enviro = new Mock<IEnvironmentVariables>();
            var objectR = new Mock<IObjectRepository>();
            var misc = new Mock<IMiscRepository>();

            var services = DI.Container.Services(enviro, userInfoRepository, objectR, misc, email);

            var function = new CreateUser(services);
            var context = new TestLambdaContext();
            var result = function.CreateUserHandler(request, context);

            var oKResponse = result.Body;

            Assert.Equal("User record created and secret emailed", oKResponse);

            userInfoRepository.Verify(k => k.Load(It.IsAny<string>()), Times.Once());
            userInfoRepository.Verify(k => k.Save(It.IsAny<UserInformation>()), Times.Once());

            // email.Verify(k => k.EmailSecret(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>()), Times.Once());
        }

        [Fact]
        public void CreateUserHandler_WhenCalledWithNewUserTwitter_CreatesUser()
        {
            APIGatewayProxyRequest request;
            var headers = new Dictionary<string, string>()
            {
            };
            request = new APIGatewayProxyRequest
            {
                Body = UserInputTwitter,
            };

            UserInformation info = null;

            var userInfoRepository = new Mock<IUserRepository>();
            userInfoRepository.Setup(x => x.Load(It.IsAny<string>())).Returns(info);
            var email = new Mock<IEmail>();
            var enviro = new Mock<IEnvironmentVariables>();
            var objectR = new Mock<IObjectRepository>();
            var misc = new Mock<IMiscRepository>();

            var services = DI.Container.Services(enviro, userInfoRepository, objectR, misc, email);

            var function = new CreateUser(services);
            var context = new TestLambdaContext();
            var result = function.CreateUserHandler(request, context);

            var oKResponse = result.Body;

            Assert.Equal("User record created and secret emailed", oKResponse);

            userInfoRepository.Verify(k => k.Load(It.IsAny<string>()), Times.Once());
            userInfoRepository.Verify(k => k.Save(It.IsAny<UserInformation>()), Times.Once());

            // email.Verify(k => k.EmailSecret(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>()), Times.Once());
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
                Body = UserInput,
            };

            UserInformation info = new UserInformation
            {
                Username = "Drumzey",
            };

            var userInfoRepository = new Mock<IUserRepository>();
            userInfoRepository.Setup(x => x.Load(It.IsAny<string>())).Returns(info);

            var email = new Mock<IEmail>();
            var enviro = new Mock<IEnvironmentVariables>();
            var objectR = new Mock<IObjectRepository>();
            var misc = new Mock<IMiscRepository>();

            var services = DI.Container.Services(enviro, userInfoRepository, objectR, misc, email);

            var function = new CreateUser(services);
            var context = new TestLambdaContext();
            var result = function.CreateUserHandler(request, context);

            var failureResponse = result.Body;

            Assert.Equal("Username already exists", failureResponse);

            userInfoRepository.Verify(k => k.Load(It.IsAny<string>()), Times.Once());
            userInfoRepository.Verify(k => k.Save(It.IsAny<UserInformation>()), Times.Never);

            // email.Verify(k => k.EmailSecret(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>()), Times.Never);
        }
    }
}
