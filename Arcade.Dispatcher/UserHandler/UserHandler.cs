using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                    return RestoreUserAggregate(services, request, context);

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

        private static APIGatewayProxyResponse RestoreUserAggregate(
            IServiceProvider services,
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            string username;
            request.QueryStringParameters.TryGetValue("username", out username);

            var restoreUser = new RestoreUser.RestoreUser(services);
            var restoreUserResponse = restoreUser.GetUserInfo(username);

            var friendsGames = new GetScore.GetScore(services);
            var friendsGamesResponse = new List<UserInformation>();
            if (restoreUserResponse.Friends.Count > 0)
            {
                friendsGamesResponse = friendsGames.GetUserInfo(string.Join(',', restoreUserResponse.Friends));
            }

            return RestoreUserResponse(restoreUserResponse, friendsGamesResponse);
        }

        private static APIGatewayProxyResponse RestoreUserResponse(
            UserInformation restoreUserResponse,
            List<UserInformation> friendsGamesResponse)
        {
            var response = new Dictionary<string, object>();
            response.Add("Me", restoreUserResponse);

            GetUserInformationResponse friendResponse = new GetUserInformationResponse();
            if (friendsGamesResponse != null)
            {
                friendResponse.Users = new List<GetSingleInformationResponse>();

                foreach (UserInformation info in friendsGamesResponse)
                {
                    friendResponse.Users.Add(new GetSingleInformationResponse
                    {
                        Username = info.Username,
                        Games = info.Games,
                        Ratings = info.Ratings,
                        Clubs = info.Clubs,
                    });
                }
            }

            response.Add("Friends", friendResponse);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(response)
            };
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
