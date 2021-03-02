using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System;
using System.Net;

namespace Arcade.Dispatcher.UserHandler
{
    public static class UserHandler
    {
        public static APIGatewayProxyResponse HandleRequest(
            APIGatewayProxyRequest request,
            ILambdaContext context,
            IServiceProvider services)
        {
            switch (request.Resource)
            {
                case "/app/users/all":
                    var allUsers = new GetUsers.GetUsers(services);
                    return allUsers.GetUsersHandler(request, context);

                case "/app/users/create":
                    var createUser = new CreateUser.CreateUser(services);
                    return createUser.CreateUserHandler(request, context);

                case "/app/users/forgot":
                    var forgotInfo = new ForgotInformation.ForgotInformation(services);
                    return forgotInfo.ForgotInformationHandler(request, context);

                case "/app/users/messages":
                    if (request.HttpMethod == "GET")
                    {
                        var myMessages = new MyMessages_GET.MyMessages_GET(services);
                        return myMessages.MyMessages_GETHandler(request, context);
                    }
                    else if (request.HttpMethod == "POST")
                    {
                        var challenges = new Challenges_POST.Challenges_POST(services);
                        return challenges.Challenges_POSTHandler(request, context);
                    }
                    return ErrorResponse("Unknown method on user messages.");

                case "/app/users/profile":
                    if(request.HttpMethod == "GET")
                    {
                        var profileGet = new ProfileStats_GET.ProfileStats_GET(services);
                        return profileGet.ProfileStats_GETHandler(request, context);
                    }
                    else if (request.HttpMethod == "POST")
                    {
                        var profilePost = new ProfileStats_POST.ProfileStats_POST(services);
                        return profilePost.ProfileStats_POSTHandler(request, context);
                    }
                    return ErrorResponse("Unknown method on user profile.");

                case "/app/users/restore":
                    var restoreUser = new RestoreUser.RestoreUser(services);
                    return restoreUser.RestoreUserHandler(request, context);

                case "/app/users/update":
                    var updateUser = new UpdateUserInfo.UpdateUserInfo(services);
                    return updateUser.UpdateUserInfoHandler(request, context);

                case "/app/users/verify":
                    var verifyUser = new VerifyUser.VerifyUser(services);
                    return verifyUser.VerifyUserHandler(request, context);

                default:
                    return ErrorResponse("Unknown User endpoint.");
            }
        }

        private static APIGatewayProxyResponse ErrorResponse(string errorMessage)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"Error. " + errorMessage + "\"}",
            };
        }
    }
}
