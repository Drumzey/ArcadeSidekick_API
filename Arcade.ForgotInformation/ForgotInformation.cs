using System;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Repositories;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.ForgotInformation
{
    public class ForgotInformation
    {
        private IServiceProvider services;

        public ForgotInformation()
            : this(DI.Container.Services())
        {
        }

        public ForgotInformation(IServiceProvider services)
        {
            this.services = services;
            ((IUserRepository)this.services.GetService(typeof(IUserRepository))).SetupTable();
        }

        public APIGatewayProxyResponse ForgotInformationHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string mode;
            request.QueryStringParameters.TryGetValue("mode", out mode);

            if (string.IsNullOrEmpty(mode))
            {
                return ErrorResponse("No mode given");
            }

            APIGatewayProxyResponse response = null;

            switch (mode)
            {
                case "USERNAME":
                    string email;
                    request.QueryStringParameters.TryGetValue("Email", out email);

                    if (string.IsNullOrEmpty(email))
                    {
                        return ErrorResponse("No email given");
                    }

                    response = GetUsernameReminder(email);
                    break;

                case "EMAIL":
                    string username;
                    request.QueryStringParameters.TryGetValue("Username", out username);

                    if (string.IsNullOrEmpty(username))
                    {
                        return ErrorResponse("No user name given");
                    }

                    response = GetEmailReminder(username);
                    break;

                case "SECRET":

                    string username2;
                    request.QueryStringParameters.TryGetValue("Username", out username2);

                    if (string.IsNullOrEmpty(username2))
                    {
                        return ErrorResponse("No user name given");
                    }

                    string email2;
                    request.QueryStringParameters.TryGetValue("Email", out email2);

                    if (string.IsNullOrEmpty(email2))
                    {
                        return ErrorResponse("No email given");
                    }

                    response = GetSecretReminder(username2, email2);
                    break;

                default:
                    return ErrorResponse("Unknown request");
            }

            return response;
        }

        private APIGatewayProxyResponse GetSecretReminder(string username, string email)
        {
            var repository = (IUserRepository)services.GetService(typeof(IUserRepository));
            var user = repository.Load(username);

            if (user == null)
            {
                return ErrorResponse("User not found");
            }

            if (user.EmailAddress != email)
            {
                return ErrorResponse("Email does not match");
            }

            var environment = (IEnvironmentVariables)services.GetService(typeof(IEnvironmentVariables));

            Email mail = new Email();
            mail.EmailSecretReminder(user.Secret, email, username);
            return OkResponse("Email Sent");
        }

        private APIGatewayProxyResponse GetEmailReminder(string username)
        {
            var repository = (IUserRepository)services.GetService(typeof(IUserRepository));
            var user = repository.Load(username);

            if (user == null)
            {
                return ErrorResponse("User not found");
            }

            return OkResponse(user.EmailAddress.EmailReminder());
        }

        private APIGatewayProxyResponse GetUsernameReminder(string email)
        {
            var environment = (IEnvironmentVariables)services.GetService(typeof(IEnvironmentVariables));

            Email mail = new Email();
            mail.EmailUsernameReminder(email);
            return OkResponse("Reminder request made");
        }

        private APIGatewayProxyResponse OkResponse(string value)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "{ \"message\": \"" + value + "\"}",
            };
        }

        private APIGatewayProxyResponse ErrorResponse(string value)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"" + value + "\"}",
            };
        }
    }
}
