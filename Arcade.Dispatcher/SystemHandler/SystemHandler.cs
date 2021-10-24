using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Locations;
using Arcade.Shared.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace Arcade.Dispatcher.SystemHandler
{
    public static class SystemHandler
    {
        public static APIGatewayProxyResponse HandleRequest(
            APIGatewayProxyRequest request,
            ILambdaContext context,
            IServiceProvider services)
        {
            switch (request.Resource)
            {
                case "/app/system/startup/firsttime":
                    return StartupAggregateFirstTime(services, request, context);

                case "/app/system/startup/unregistereduser":
                    return StartupAggregateUnregisteredUser(services, request, context);

                case "/app/system/startup/registereduser":
                    return StartupAggregateRegisterdUser(services, request, context);

                case "/app/system/activity":
                    var activity = new RecentActivity.RecentActivity(services);
                    return activity.RecentActivityHandler(request, context);

                default:
                    return ErrorResponse("Unknown System endpoint.");
            }
        }

        private static APIGatewayProxyResponse StartupAggregateFirstTime(
            IServiceProvider services,
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            var clubs = ClubHandler.ClubHandler.GetAllClubs(request, context, services);
            var venues = VenueHandler.VenuesHandler.GetAllVenues(request, context, services);
            
            return StartupFirstTimeResponse(clubs, venues);
        }

        private static APIGatewayProxyResponse StartupAggregateUnregisteredUser(
            IServiceProvider services,
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            request.QueryStringParameters.TryGetValue("friends", out string friendsName);

            var clubs = ClubHandler.ClubHandler.GetAllClubs(request, context, services);
            var venues = VenueHandler.VenuesHandler.GetAllVenues(request, context, services);

            var friendsGames = new GetScore.GetScore(services);
            var friends = friendsGames.GetUserInfo(friendsName);

            return StartupUnregisteredResponse(clubs, venues, friends);
        }

        private static APIGatewayProxyResponse StartupAggregateRegisterdUser(
            IServiceProvider services,
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            request.QueryStringParameters.TryGetValue("username", out string userName);

            request.QueryStringParameters.TryGetValue("friends", out string friendsName);

            var clubs = ClubHandler.ClubHandler.GetAllClubs(request, context, services);
            var venues = VenueHandler.VenuesHandler.GetAllVenues(request, context, services);

            var myMessages = new MyMessages_GET.MyMessages_GET(services);
            var messages = myMessages.GetMessages(services, userName, request);

            List<UserInformation> friends = new List<UserInformation>();

            // If we have friends then get their scores.
            if (!string.IsNullOrEmpty(friendsName))
            {
                var friendsGames = new GetScore.GetScore(services);
                friends = friendsGames.GetUserInfo(friendsName);
            }

            var profileGet = new ProfileStats_GET.ProfileStats_GET(services);
            var stats = profileGet.GetUserInfo(userName);

            return StartupRegisteredUserResponse(clubs, venues, friends, messages.New, messages.Old, stats);
        }

        private static APIGatewayProxyResponse StartupFirstTimeResponse(
            List<ClubInformation> clubInfo,
            List<Location> locationInfo)
        {
            var response = new Dictionary<string, object>
            {
                { "Clubs", clubInfo },
                { "Venues", locationInfo }
            };

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(response)
            };
        }

        private static APIGatewayProxyResponse StartupUnregisteredResponse(
            List<ClubInformation> clubInfo,
            List<Location> locationInfo,
            List<UserInformation> friendsInfo)
        {
            var response = new Dictionary<string, object>
            {
                { "Clubs", clubInfo },
                { "Venues", locationInfo }
            };

            GetUserInformationResponse friendResponse = new GetUserInformationResponse();
            if (friendsInfo != null)
            {
                friendResponse.Users = new List<GetSingleInformationResponse>();

                foreach (UserInformation info in friendsInfo)
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

        private static APIGatewayProxyResponse StartupRegisteredUserResponse(
            List<ClubInformation> clubInfo,
            List<Location> locationInfo,
            List<UserInformation> friendsInfo,
            List<Message> newMessages,
            List<Message> oldMessages,
            UserInformation stats)
        {
            var response = new Dictionary<string, object>
            {
                { "Clubs", clubInfo },
                { "Venues", locationInfo },
                { "NewMessages", newMessages },
                { "OldMessages", oldMessages },
                { "Stats", stats }
            };

            GetUserInformationResponse friendResponse = new GetUserInformationResponse();
            if (friendsInfo != null)
            {
                friendResponse.Users = new List<GetSingleInformationResponse>();

                foreach (UserInformation info in friendsInfo)
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
