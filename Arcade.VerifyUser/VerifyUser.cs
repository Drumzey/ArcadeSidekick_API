using System;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;
using TweetSharp;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.VerifyUser
{
    public class VerifyUser
    {
        private IServiceProvider services;
        private IEnvironmentVariables environmentVariables;

        public VerifyUser()
            : this(DI.Container.Services())
        {
        }

        public VerifyUser(IServiceProvider services)
        {
            this.services = services;
            environmentVariables = (IEnvironmentVariables)this.services.GetService(typeof(IEnvironmentVariables));
            ((IUserRepository)this.services.GetService(typeof(IUserRepository))).SetupTable();
        }

        public APIGatewayProxyResponse VerifyUserHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var userInfo = JsonConvert.DeserializeObject<CreateUserInformation>(request.Body);
            var user = ((IUserRepository)services.GetService(typeof(IUserRepository))).Load(userInfo.Username);

            if (user == null)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Body = "{ \"message\": \"User record not found.\" }",
                };
            }

            if (user.Verified == false)
            {
                try
                {
                    // We have a new user so lets tweet!
                    var service = new TwitterService(environmentVariables.ConsumerAPIKey, environmentVariables.ConsumerAPISecretKey);
                    service.AuthenticateWith(environmentVariables.AccessToken, environmentVariables.AccessTokenSecret);

                    var message = string.Empty;

                    if (string.IsNullOrEmpty(user.TwitterHandle))
                    {
                        message = $"A new user has joined the fun! Welcome {user.Username}! #arcade #highscore #retrogaming";
                    }
                    else
                    {
                        message = $"A new user has joined the fun! Welcome {user.Username} (@{user.TwitterHandle})! #arcade #highscore #retrogaming";
                    }

                    service.SendTweet(new SendTweetOptions
                    {
                        Status = message,
                    });
                }
                catch (Exception e)
                {
                    // Oh dear
                }
            }

            user.Verified = true;
            ((IUserRepository)services.GetService(typeof(IUserRepository))).Save(user);

            return OkResponse();
        }

        private APIGatewayProxyResponse OkResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "{ \"message\": \"User record verified.\" }",
            };
        }
    }
}
