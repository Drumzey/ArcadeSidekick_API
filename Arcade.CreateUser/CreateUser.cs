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

namespace Arcade.CreateUser
{
    public class CreateUser
    {

        private IServiceProvider _services;

        public CreateUser()
            : this(DI.Container.Services())
        {
        }

        public CreateUser(IServiceProvider services)
        {
            _services = services;
            ((IUserRepository)_services.GetService(typeof(IUserRepository))).SetupTable();
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
                return OkResponse();
            }
            catch(Exception e)
            {
                return ErrorResponse(e.Message);
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
            };

            ((IUserRepository)_services.GetService(typeof(IUserRepository))).Save(newUser);
            var email = ((IEmail)_services.GetService(typeof(IEmail)));            
            email.EmailSecret(newUser.Secret, newUser.EmailAddress);
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
            var user = ((IUserRepository)_services.GetService(typeof(IUserRepository))).Load(username);
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
