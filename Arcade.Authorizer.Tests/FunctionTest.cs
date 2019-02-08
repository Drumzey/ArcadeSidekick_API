using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using Arcade.Authorizer;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Moq;
using Amazon.Lambda.APIGatewayEvents;

namespace Arcade.Authorizer.Tests
{
    public class FunctionTest
    {
        string JWT_signed_with_MySecret = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJEUlVNWkVZIiwiaXNzIjoiU2lkZWtpY2siLCJleHAiOjE1NDk0NjUxNjQsImlhdCI6MTU0OTQ2NDg2NH0.ss279xfd2tYcy9-KrXY0UGkGFYh9sSOs6h8Aw8vHsrM";
        string JWT_wrong_issuers = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJEUlVNWkVZIiwiaXNzIjoiT3RoZXIiLCJleHAiOjE1NDk0NjQ1MDksImlhdCI6MTU0OTQ2NDIwOX0.HcAPOGliOif6IGmz9qq7N40RJ3_ODfMzaYRVMzceZjY";
        string JWT_tampered = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJHVVNGVSIsImlzcyI6IlNpZGVraWNrIiwiZXhwIjoxNTQ5NDY0NTA5LCJpYXQiOjE1NDk0NjQyMDl9.Y8wtsmfgtzS-E5oIaT2yD0LDgffCYYShmHbPBlNxnKQ";
        string JWT_withNoClaims = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.e30.JsU41fXf6KwG8qhVpWCvevARtRFf_kJxDyv1LpEpcpE";

        [Fact]
        public void TestValidJWT()
        {
            UserInformation info = new UserInformation
            {
                Username = "Drumzey",
                EmailAddress = "Drumzey@test.com",
                Secret = "E1Fd81iBMlqo3odiN+0vp0VQ8L8UjZGqSjw+fZRMOQo=",
                Games = new Dictionary<string, string>(),
                Ratings = new Dictionary<string, int>(),
                Verified = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var userInfoRepository = new Mock<IUserRepository>();
            userInfoRepository.Setup(x => x.Load("DRUMZEY")).Returns(info);

            var services = DI.Container.Services(null, userInfoRepository);

            var function = new Authorizer(services);
            var context = new TestLambdaContext();

            var tokenContext = SetupTokenContext(JWT_signed_with_MySecret);
            var result = function.AuthorizerHandler(tokenContext, context);

            var policyStatement = result.PolicyDocument.Statement[0];

            Assert.Equal("Sidekick", result.PrincipalID);
            Assert.Equal("Allow", policyStatement.Effect);
            Assert.Subset(policyStatement.Resource, new HashSet<string>() { "*" });
        }

        [Fact]
        public void TestIncorrectIssuerJWT()
        {
            UserInformation info = new UserInformation
            {
                Username = "Drumzey",
                EmailAddress = "Drumzey@test.com",
                Secret = "E1Fd81iBMlqo3odiN+0vp0VQ8L8UjZGqSjw+fZRMOQo=",
                Games = new Dictionary<string, string>(),
                Ratings = new Dictionary<string, int>(),
                Verified = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var userInfoRepository = new Mock<IUserRepository>();
            userInfoRepository.Setup(x => x.Load("DRUMZEY")).Returns(info);

            var services = DI.Container.Services(null, userInfoRepository);

            var function = new Authorizer(services);
            var context = new TestLambdaContext();

            var tokenContext = SetupTokenContext(JWT_wrong_issuers);
            Assert.Throws<Exception>(() => function.AuthorizerHandler(tokenContext, context));
        }

        [Fact]
        public void TestTamperedJWT()
        {
            UserInformation info = new UserInformation
            {
                Username = "Drumzey",
                EmailAddress = "Drumzey@test.com",
                Secret = "E1Fd81iBMlqo3odiN+0vp0VQ8L8UjZGqSjw+fZRMOQo=",
                Games = new Dictionary<string, string>(),
                Ratings = new Dictionary<string, int>(),
                Verified = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var userInfoRepository = new Mock<IUserRepository>();
            userInfoRepository.Setup(x => x.Load("DRUMZEY")).Returns(info);

            var services = DI.Container.Services(null, userInfoRepository);

            var function = new Authorizer(services);
            var context = new TestLambdaContext();

            var tokenContext = SetupTokenContext(JWT_tampered);
            Assert.Throws<Exception>(() => function.AuthorizerHandler(tokenContext, context));
        }

        [Fact]
        public void TestNoClaimsJWT()
        {
            UserInformation info = new UserInformation
            {
                Username = "Drumzey",
                EmailAddress = "Drumzey@test.com",
                Secret = "E1Fd81iBMlqo3odiN+0vp0VQ8L8UjZGqSjw+fZRMOQo=",
                Games = new Dictionary<string, string>(),
                Ratings = new Dictionary<string, int>(),
                Verified = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var userInfoRepository = new Mock<IUserRepository>();
            userInfoRepository.Setup(x => x.Load("DRUMZEY")).Returns(info);

            var services = DI.Container.Services(null, userInfoRepository);

            var function = new Authorizer(services);
            var context = new TestLambdaContext();

            var tokenContext = SetupTokenContext(JWT_withNoClaims);
            Assert.Throws<Exception>(() => function.AuthorizerHandler(tokenContext, context));
        }

        [Fact]
        public void TestMalformedJWT()
        {
            var userInfoRepository = new Mock<IUserRepository>();         
            var services = DI.Container.Services(null, userInfoRepository);

            var function = new Authorizer(services);
            var context = new TestLambdaContext();

            var tokenContext = SetupTokenContext("randomText");
            Assert.Throws<Exception>(() => function.AuthorizerHandler(tokenContext, context));
        }

        private APIGatewayCustomAuthorizerRequest SetupTokenContext(string jwt)
        {
            var context = new APIGatewayCustomAuthorizerRequest
            {
                Type = "token",
                AuthorizationToken = jwt,
                MethodArn = "methodarn",
            };
            return context;
        }
    }
}
