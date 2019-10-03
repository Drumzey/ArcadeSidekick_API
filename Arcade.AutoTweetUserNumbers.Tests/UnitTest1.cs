using System;
using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Moq;
using Xunit;

namespace Arcade.AutoTweetUserNumbers.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void TestTweet()
        {
            APIGatewayProxyRequest request;
            var headers = new Dictionary<string, string>()
            {
            };
            request = new APIGatewayProxyRequest();

            var userInfo = new ObjectInformation
            {
                Key = "users",
                ListValue = new List<string>
                {
                    "33",
                },
            };

            var previoususerInfo = new ObjectInformation
            {
                Key = "previoususers",
                ListValue = new List<string>
                {
                    "0",
                },
            };

            var objectRepository = new Mock<IObjectRepository>();
            objectRepository.Setup(x => x.Load("users")).Returns(userInfo);
            objectRepository.Setup(x => x.Load("previoususers")).Returns(previoususerInfo);

            var environmentVariables = new Mock<IEnvironmentVariables>();
            environmentVariables.Setup(x => x.ConsumerAPIKey).Returns("QbE6XZPG2vrSoKi3s6Vr19hjD");
            environmentVariables.Setup(x => x.ConsumerAPISecretKey).Returns("jILbhdxCD19g09e3icUG6epcq63Qy92yBBXo1BjkL3YFnJRqIi");
            environmentVariables.Setup(x => x.AccessToken).Returns("1103770114888003602-MueDVbSWMKu90APgK3do45pQpIgp3A");
            environmentVariables.Setup(x => x.AccessTokenSecret).Returns("6IgP462EtLJ91i0RlxMiUGcVK4Iga6p8FCJRq9UzR3zRb");

            var services = DI.Container.Services(environmentVariables, objectRepository);

            var function = new AutoTweetUserNumbers(services);
            var context = new TestLambdaContext();
            function.AutoTweetUserNumbersHandler(request, context);
        }
    }
}
