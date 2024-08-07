using System;
using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared.Misc;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.GetUsers
{
    public class GetUsers
    {
        private IServiceProvider services;

        public GetUsers()
            : this(DI.Container.Services())
        {
        }

        public GetUsers(IServiceProvider services)
        {
            this.services = services;
            ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).SetupTable();
        }

        public APIGatewayProxyResponse GetUsersHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var users = ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Load("Activity", "Users");
                return OkResponse(users.List1);
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        private APIGatewayProxyResponse OkResponse(List<string> users)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(users),
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
