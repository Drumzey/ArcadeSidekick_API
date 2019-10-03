using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared.LocationByMachine;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.WhereCanIPlay_GET
{
    public class WhereCanIPlay_GET
    {
        private IServiceProvider _services;

        public WhereCanIPlay_GET()
            : this(DI.Container.Services())
        {
        }

        public WhereCanIPlay_GET(IServiceProvider services)
        {
            _services = services;
            ((ILocationByMachineRepository)_services.GetService(typeof(ILocationByMachineRepository))).SetupTable();
        }

        public APIGatewayProxyResponse WhereCanIPlay_GETHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var gameName = request.QueryStringParameters["gamename"];
            if (string.IsNullOrEmpty(gameName))
            {
                return ErrorResponse();
            }

            var locations = ((ILocationByMachineRepository)_services.
                GetService(typeof(ILocationByMachineRepository))).Load(gameName);

            if (locations == null)
            {
                return NotFoundResponse();
            }

            return LocationResponse(locations);
        }

        private APIGatewayProxyResponse LocationResponse(LocationByMachine location)
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
                Body = "{ \"message\": \"No known locations for this game.\"}",
            };
        }

        private APIGatewayProxyResponse ErrorResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"Error. Cannot get settings for game.\"}",
            };
        }
    }
}
