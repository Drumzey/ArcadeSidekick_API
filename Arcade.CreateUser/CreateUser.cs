using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.CreateUser
{
    public class CreateUser
    {
        private IServiceProvider services;

        public CreateUser()
            : this(DI.Container.Services())
        {
        }

        public CreateUser(IServiceProvider services)
        {
            this.services = services;
            ((IUserRepository)this.services.GetService(typeof(IUserRepository))).SetupTable();
            ((IObjectRepository)this.services.GetService(typeof(IObjectRepository))).SetupTable();
        }

        public APIGatewayProxyResponse CreateUserHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var userInfo = JsonConvert.DeserializeObject<CreateUserInformation>(request.Body);

                if (DoesUserExist(userInfo.Username))
                {
                    return ConflictResponse();
                }

                CreateUserInDatabase(userInfo);
                CreateUserInObjectTable(userInfo.Username);
                SaveRecentActivity(userInfo.Username);
                return OkResponse();
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        private void SaveRecentActivity(string username)
        {
            var recentActivity = ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Load("recent");

            if (recentActivity == null)
            {
                recentActivity = new ObjectInformation();
                recentActivity.Key = "recent";
                recentActivity.ListValue = new System.Collections.Generic.List<string>();
            }

            var message = GetMessage(username, DateTime.UtcNow.ToString("dd/MM/yyyy h:mm tt"));

            Console.WriteLine(message);

            recentActivity.ListValue.Add(message);

            var newList = recentActivity.ListValue.Skip(Math.Max(0, recentActivity.ListValue.Count() - 50));
            recentActivity.ListValue = newList.ToList();

            ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Save(recentActivity);
        }

        private string GetMessage(string username, string date)
        {
            return $"{date}: {username.ToUpper()} joined the fun!";
        }

        private void CreateUserInObjectTable(string userName)
        {
            try
            {
                var users = ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Load("users");

                if (users == null)
                {
                    users = new ObjectInformation();
                    users.Key = "users";
                    users.ListValue = new List<string>();
                }

                users.ListValue.Add(userName);

                ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Save(users);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void CreateUserInDatabase(CreateUserInformation userInfo)
        {
            UserInformation newUser = new UserInformation
            {
                Username = userInfo.Username,
                EmailAddress = userInfo.EmailAddress,
                Secret = GenerateSecret(),
                Games = new Dictionary<string, string>(),
                Ratings = new Dictionary<string, int>(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Verified = false,
                NumberOfChallengesSent = 0,
                NumberOfGamesPlayed = 0,
                NumberOfRatingsGiven = 0,
                NumberOfScoresUploaded = 0,
                NumberOfSocialShares = 0,
                TwitterHandle = string.Empty,
                Location = string.Empty,
            };

            ((IUserRepository)services.GetService(typeof(IUserRepository))).Save(newUser);
            var email = (IEmail)services.GetService(typeof(IEmail));
            var environment = (IEnvironmentVariables)services.GetService(typeof(IEnvironmentVariables));

            email.EmailSecret(newUser.Secret, newUser.EmailAddress, newUser.Username, environment);
        }

        private string GenerateSecret()
        {
            RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            byte[] randomBytes = new byte[32];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private bool DoesUserExist(string username)
        {
            var user = ((IUserRepository)services.GetService(typeof(IUserRepository))).Load(username);
            return user != null;
        }

        private APIGatewayProxyResponse OkResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "{ \"message\": \"User record created and secret emailed\"}",
            };
        }

        private APIGatewayProxyResponse ConflictResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Conflict,
                Body = "{ \"message\": \"Username already exists\"}",
            };
        }

        private APIGatewayProxyResponse ErrorResponse(string error)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = "{ \"message\": \"Error. " + error + "\"}",
            };
        }
    }
}
