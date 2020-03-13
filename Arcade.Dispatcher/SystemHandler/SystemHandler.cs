using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System;
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
                case "/app/system/activity":
                    var activity = new RecentActivity.RecentActivity(services);
                    return activity.RecentActivityHandler(request, context);

                default:
                    return ErrorResponse("Unknown System endpoint.");
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
