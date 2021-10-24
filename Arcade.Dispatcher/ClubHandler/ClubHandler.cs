using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using System;
using System.Collections.Generic;
using System.Net;

namespace Arcade.Dispatcher.ClubHandler
{
    public static class ClubHandler
    {
        public static APIGatewayProxyResponse HandleRequest(
            APIGatewayProxyRequest request, 
            ILambdaContext context,
            IServiceProvider services)
        {
            switch(request.Resource)
            {
                case "/app/clubs/all":
                    var getClubs = new GetClubs.GetClubs(services);
                    return getClubs.GetClubsHandler(request, context);

                case "/app/clubs/join":
                    var joinClub = new JoinClub.JoinClub(services);
                    return joinClub.JoinClubHandler(request, context);

                case "/app/clubs/leave":
                    var leaveClub = new LeaveClub.LeaveClub(services);
                    return leaveClub.LeaveClubHandler(request, context);

                case "/app/clubs/events":
                case "/app/clubs/invite":
                case "/app/clubs/news":
                    var events = new EventNews_POST.EventNews_POST(services);
                    return events.EventNews_POSTHandler(request, context);                
                
                case "/app/clubs/request":
                    return ErrorResponse("Not implemented");
                default:
                    return ErrorResponse("Unknown Club endpoint");
            }
        }

        public static List<ClubInformation> GetAllClubs(
            APIGatewayProxyRequest request,
            ILambdaContext context,
            IServiceProvider services)
        {
            var getClubs = new GetClubs.GetClubs(services);
            return getClubs.GetClubsInformation();
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
