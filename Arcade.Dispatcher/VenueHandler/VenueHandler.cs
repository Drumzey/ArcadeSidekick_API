using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System;
using System.Net;

namespace Arcade.Dispatcher.VenueHandler
{
    public static class VenuesHandler
    {
        public static APIGatewayProxyResponse HandleRequest(
            APIGatewayProxyRequest request,
            ILambdaContext context,
            IServiceProvider services)
        {
            var locations = new Locations.Locations(services);

            switch (request.Resource)
            {
                case "/app/venues/all":
                case "/app/venues/join":
                case "/website/venues":
                    return locations.LocationsHandler(request, context);
                default:
                    return ErrorResponse("Unknown location endpoint.");
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
