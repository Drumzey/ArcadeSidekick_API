using System;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared.Locations;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.Location_POST
{
    public class Location_POST
    {
        private IServiceProvider _services;

        public Location_POST()
            : this(DI.Container.Services())
        {
        }

        public Location_POST(IServiceProvider services)
        {
            _services = services;
            ((ILocationRepository)_services.GetService(typeof(ILocationRepository))).SetupTable();
        }

        public APIGatewayProxyResponse Location_POSTHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var data = JsonConvert.DeserializeObject<Location>(request.Body);

            if (string.IsNullOrEmpty(data.LocationName))
            {
                return ErrorResponse("No location name given");
            }

            var location = ((ILocationRepository)_services
                .GetService(typeof(ILocationRepository))).Load(data.LocationName);

            if (location != null)
            {
                return LocationResponse(location);
            }

            location = new Location
            {
                LocationName = data.LocationName,
                Address = data.Address,
                Website = data.Website,
                Information = data.Information,
                SubmissionRules = data.SubmissionRules,
            };

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
        
        private APIGatewayProxyResponse ErrorResponse(string message)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"Error. " + message + ".\"}",
            };
        }
    }
}
