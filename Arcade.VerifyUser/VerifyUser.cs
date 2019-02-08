using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.VerifyUser
{
    public class VerifyUser
    {
        private IServiceProvider _services;

        public VerifyUser()
            : this(DI.Container.Services())
        {
        }

        public VerifyUser(IServiceProvider services)
        {
            _services = services;
            ((IUserRepository)_services.GetService(typeof(IUserRepository))).SetupTable();
        }

        public APIGatewayProxyResponse VerifyUserHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var userInfo = JsonConvert.DeserializeObject<CreateUserInformation>(request.Body);
            var user = ((IUserRepository)_services.GetService(typeof(IUserRepository))).Load(userInfo.Username);

            if(user == null)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Body = "{ \"message\": \"User record not found.\" }",
                };
            }

            user.Verified = true;
            ((IUserRepository)_services.GetService(typeof(IUserRepository))).Save(user);
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
