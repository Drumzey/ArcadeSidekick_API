using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Locations;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.Locations
{
    public class Locations
    {
        private IServiceProvider services;
        private IEnvironmentVariables environmentVariables;
        private ILocationRepository locationRepository;

        public Locations()
            : this(DI.Container.Services())
        {
        }

        public Locations(IServiceProvider services)
        {
            this.services = services;
            environmentVariables = (IEnvironmentVariables)this.services.GetService(typeof(IEnvironmentVariables));
            locationRepository = (ILocationRepository)this.services.GetService(typeof(ILocationRepository));
            locationRepository.SetupTable();
        }

        public APIGatewayProxyResponse LocationsHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            object response;
            switch (request.Resource)
            {
                case "/app/venues/all":
                    response = GetLocations();
                    break;

                default:
                    return ErrorResponse();
            }

            return Response(response);
        }

        private List<string> GetLocations()
        {
            var games = locationRepository.AllLocations();
            var locations = games.Select(x => x.Name).ToList();
            return locations;
        }

        private APIGatewayProxyResponse Response(object returnObject)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(returnObject),
            };
        }

        private APIGatewayProxyResponse ErrorResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"Error getting location details\" }",
            };
        }
    }
}
