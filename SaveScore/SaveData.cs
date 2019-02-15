using System;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using System.IdentityModel.Tokens.Jwt;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.SaveScore
{
    public class SaveScore
    {
        private IServiceProvider _services;

        public SaveScore()
            : this(DI.Container.Services())
        {
        }

        public SaveScore(IServiceProvider services)
        {
            _services = services;
            ((IUserRepository)_services.GetService(typeof(IUserRepository))).SetupTable();            
        }

        public APIGatewayProxyResponse SaveScoreHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var userInfo = SaveUserInfo(request.Body, request.Headers["Authorization"]);
            return Response(userInfo);
        }

        private UserInformation SaveUserInfo(string requestBody, string token)
        {
            var data = JsonConvert.DeserializeObject<SaveUserInformationInput>(requestBody);

            JwtSecurityToken jwtToken;
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            try
            {
                jwtToken = handler.ReadJwtToken(token);
            }
            catch (Exception e)
            {
                throw new Exception("Invalid JWT");
            }

            if (jwtToken.Id.ToLower() != data.Username.ToLower())
            {
                throw new Exception("Attempting to Save data for other user");
            }

            var userInformation = ((IUserRepository)_services.GetService(typeof(IUserRepository))).Load(data.Username);

            if (userInformation == null)
            {
                userInformation = new UserInformation
                {
                    Username = data.Username,
                    Games = data.Games,
                    Ratings = data.Ratings,
                    CreatedAt = DateTime.Now,
                };
            }
            else
            {
                //Take the scores and ratings and update each game
                foreach (string gameKey in data.Games.Keys)
                {
                    if (userInformation.Games.ContainsKey(gameKey))
                    {
                        userInformation.Games[gameKey] = data.Games[gameKey];
                        userInformation.Ratings[gameKey] = data.Ratings[gameKey];
                    }
                    else
                    {
                        userInformation.Games.Add(gameKey, data.Games[gameKey]);
                        userInformation.Ratings.Add(gameKey, data.Ratings[gameKey]);
                    }
                }
            }

            userInformation.UpdatedAt = DateTime.Now;
            ((IUserRepository)_services.GetService(typeof(IUserRepository))).Save(userInformation);

            return userInformation;
        }

        private APIGatewayProxyResponse Response(UserInformation userInfo)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(userInfo),
            };
        }
    }
}
