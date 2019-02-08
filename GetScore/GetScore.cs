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

namespace Arcade.GetScore
{
    public class GetScore
    {
        private IServiceProvider _services;

        public GetScore()
            : this(DI.Container.Services())
        {
        }

        public GetScore(IServiceProvider services)
        {
            _services = services;
            ((IUserRepository)_services.GetService(typeof(IUserRepository))).SetupTable();
        }

        public APIGatewayProxyResponse GetScoreHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string usernames;
            request.QueryStringParameters.TryGetValue("usernames", out usernames);
            
            if (string.IsNullOrEmpty(usernames))
            {
                return ErrorResponse();
            }

            var userInfo = GetUserInfo(usernames);
            return Response(userInfo);
        }

        private List<UserInformation> GetUserInfo(string usernames)
        {
            var names = usernames.Split(',');
            var information = new List<UserInformation>();
            var userRepository = (IUserRepository)_services.GetService(typeof(IUserRepository));

            foreach (string name in names)
            {
                var userInformation = userRepository.Load(name);
                if (userInformation != null)
                {
                    information.Add(userInformation);
                }
            }
            return information;
        }

        private APIGatewayProxyResponse Response(List<UserInformation> userInfo)
        {
            GetUserInformationResponse response = new GetUserInformationResponse();

            if (userInfo != null)
            {
                response.Users = new List<GetSingleUserInformationResponse>();

                foreach(UserInformation info in userInfo)
                {
                    response.Users.Add(new GetSingleUserInformationResponse
                    {
                        Username = info.Username,
                        Games = info.Games,
                        Ratings = info.Ratings,
                    });
                }
            }

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(response),
            };
        }

        private APIGatewayProxyResponse ErrorResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"Error no usernames supplied\" }",
            };
        }
    }
}
