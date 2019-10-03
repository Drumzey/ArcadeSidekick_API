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

namespace Arcade.RestoreUser
{
    public class RestoreUser
    {
        private IServiceProvider services;

        public RestoreUser()
            : this(DI.Container.Services())
        {
        }

        public RestoreUser(IServiceProvider services)
        {
            this.services = services;
            ((IUserRepository)this.services.GetService(typeof(IUserRepository))).SetupTable();
        }

        public APIGatewayProxyResponse RestoreUserHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string username;
            request.QueryStringParameters.TryGetValue("username", out username);

            if (string.IsNullOrEmpty(username))
            {
                return ErrorResponse();
            }

            var userInfo = GetUserInfo(username);
            return Response(userInfo);
        }

        private UserInformation GetUserInfo(string username)
        {
            var userRepository = (IUserRepository)services.GetService(typeof(IUserRepository));
            var userInformation = userRepository.Load(username);

            userInformation.NumberOfGamesPlayed = userInformation.Games.Count();
            userInformation.NumberOfRatingsGiven = userInformation.Ratings.Where(x => x.Value != 0).Count();

            if (userInformation.NumberOfScoresUploaded == 0)
            {
                userInformation.NumberOfScoresUploaded = userInformation.Games.Where(x => x.Value != "0").Count();
            }

            userRepository.Save(userInformation); 

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
