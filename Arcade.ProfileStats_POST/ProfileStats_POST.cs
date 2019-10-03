using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.ProfileStats_POST
{
    public class ProfileStats_POST
    {
        private IServiceProvider services;

        public ProfileStats_POST()
            : this(DI.Container.Services())
        {
        }

        public ProfileStats_POST(IServiceProvider services)
        {
            this.services = services;
            ((IUserRepository)this.services.GetService(typeof(IUserRepository))).SetupTable();
        }

        public APIGatewayProxyResponse ProfileStats_POSTHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var data = JsonConvert.DeserializeObject<ProfileStatsUpdate>(request.Body);
            var userInfo = UpdateUserInfo(data);

            return userInfo == null ? ErrorResponse() : Response(userInfo);
        }

        private UserInformation UpdateUserInfo(ProfileStatsUpdate data)
        {
            var userRepository = (IUserRepository)services.GetService(typeof(IUserRepository));

            if (string.IsNullOrEmpty(data.Username))
            {
                return null;
            }

            var userInformation = userRepository.Load(data.Username);
            Console.Write(userInformation);
            if (userInformation != null)
            {
                if (data.TweetSent)
                {
                    userInformation.NumberOfSocialShares += 1;
                }

                if (data.ChallengeSent)
                {
                    userInformation.NumberOfChallengesSent += 1;
                }

                if (!string.IsNullOrEmpty(data.TwitterHandle))
                {
                    userInformation.TwitterHandle = data.TwitterHandle;
                }

                userRepository.Save(userInformation);
            }

            return userInformation;
        }

        private APIGatewayProxyResponse Response(UserInformation userInfo)
        {
            userInfo.Secret = "***";

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(userInfo),
            };
        }

        private APIGatewayProxyResponse ErrorResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"Error no username supplied\" }",
            };
        }
    }
}
