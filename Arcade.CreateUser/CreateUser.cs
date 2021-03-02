using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Misc;
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
            ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).SetupTable();
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
                CreateUserTwitterHandleInMiscTable(userInfo);

                try
                {
                    SaveRecentActivity(userInfo.Username);
                }
                catch (Exception)
                {
                    Console.WriteLine();
                }

                return OkResponse();
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        private void CreateUserTwitterHandleInMiscTable(CreateUserInformation userInfo)
        {
            if (string.IsNullOrWhiteSpace(userInfo.TwitterHandle))
            {
                // We have nothing then do nothing
                return;
            }

            try
            {
                var tweeties = ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Load("Twitter", "Users");

                if (tweeties == null)
                {
                    tweeties = new Misc();
                    tweeties.Key = "Twitter";
                    tweeties.SortKey = "Users";
                    tweeties.Dictionary = new Dictionary<string, string>();
                }

                if (!tweeties.Dictionary.ContainsKey(userInfo.Username))
                {
                    tweeties.Dictionary.Add(userInfo.Username, userInfo.TwitterHandle);
                }
                else
                {
                    tweeties.Dictionary[userInfo.Username] = userInfo.TwitterHandle;
                }

                ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Save(tweeties);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void SaveRecentActivity(string username)
        {
            var recentActivity = ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Load("Activity", "All");

            if (recentActivity == null)
            {
                recentActivity = new Misc();
                recentActivity.Key = "Activity";
                recentActivity.SortKey = "All";
                recentActivity.List1 = new System.Collections.Generic.List<string>();
            }

            var message = GetMessage(username, DateTime.UtcNow.ToString("dd/MM/yyyy h:mm tt"));

            Console.WriteLine(message);

            recentActivity.List1.Add(message);

            var newList = recentActivity.List1.Skip(Math.Max(0, recentActivity.List1.Count() - 50));
            recentActivity.List1 = newList.ToList();

            ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Save(recentActivity);
        }

        private string GetMessage(string username, string date)
        {
            return $"{date}: {username.ToUpper()} joined the fun!";
        }

        private void CreateUserInObjectTable(string userName)
        {
            try
            {
                var users = ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Load("Activity", "Users");

                if (users == null)
                {
                    users = new Misc();
                    users.Key = "Activity";
                    users.SortKey = "Users";
                    users.List1 = new List<string>();
                }

                users.List1.Add(userName);

                ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Save(users);
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
                TwitterHandle = userInfo.TwitterHandle,
                Location = string.Empty,
                DOB = string.Empty,
                YouTubeChannel = string.Empty,
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
