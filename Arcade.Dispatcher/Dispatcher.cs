using System;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.Dispatcher
{
    public class Dispatcher
    {
        private IServiceProvider services;
        private APIGatewayProxyRequest request;
        private ILambdaContext context;

        public Dispatcher()
            : this(DI.Container.Services())
        {
        }

        public Dispatcher(IServiceProvider services)
        {
            this.services = services;
        }

        public APIGatewayProxyResponse DispatcherHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            this.request = request;
            this.context = context;

            APIGatewayProxyResponse response;

            if (request.Resource.StartsWith("/app/clubs"))
            {
                response = HandleClubRequest();
            }
            else if (request.Resource.StartsWith("/app/games"))
            {
                response = HandleGameRequest();
            }
            else if (request.Resource.StartsWith("/app/system"))
            {
                response = HandleSystemRequest();
            }
            else if (request.Resource.StartsWith("/app/users"))
            {
                response = HandleUserRequest();
            }
            else if (request.Resource.StartsWith("/app/venues"))
            {
                response = HandleVenueRequest();
            }
            else
            {
                response = ErrorResponse("Unknown end point");
            }

            return response;
        }
        
        private APIGatewayProxyResponse ErrorResponse(string errorMessage)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"Error. " + errorMessage + "\"}",
            };
        }

        private APIGatewayProxyResponse HandleVenueRequest()
        {
            return VenueHandler.VenuesHandler.HandleRequest(request, context, services);
        }
        
        private APIGatewayProxyResponse HandleClubRequest()
        {
            return ClubHandler.ClubHandler.HandleRequest(request, context, services);
        }

        private APIGatewayProxyResponse HandleUserRequest()
        {
            return UserHandler.UserHandler.HandleRequest(request, context, services);
        }

        private APIGatewayProxyResponse HandleSystemRequest()
        {
            return SystemHandler.SystemHandler.HandleRequest(request, context, services);
        }

        private APIGatewayProxyResponse HandleGameRequest()
        {
            return GameHandler.GameHandler.HandleRequest(request, context, services);
        }
    }
}
