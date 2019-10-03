using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared.Locations;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.Location_GET
{
    public class Location_GET
    {
        private IServiceProvider _services;

        public Location_GET()
            : this(DI.Container.Services())
        {
        }

        public Location_GET(IServiceProvider services)
        {
            _services = services;            
            ((ILocationRepository)_services.GetService(typeof(ILocationRepository))).SetupTable();
        }

        public APIGatewayProxyResponse Location_GETHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var locationName = request.QueryStringParameters["location"];
            if (string.IsNullOrEmpty(locationName))
            {
                return ErrorResponse();
            }

            var location = ((ILocationRepository)_services.GetService(typeof(ILocationRepository))).Load(locationName);
            if (location == null)
            {
                return NotFoundResponse();
            }
            
            return LocationResponse(location);
        }

        private APIGatewayProxyResponse LocationResponse(Location location)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(location),
            };
        }

        private APIGatewayProxyResponse NotFoundResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Body = "{ \"message\": \"No location found.\"}",
            };
        }

        private APIGatewayProxyResponse ErrorResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"Error. Cannot get location.\"}",
            };
        }
    }
}
