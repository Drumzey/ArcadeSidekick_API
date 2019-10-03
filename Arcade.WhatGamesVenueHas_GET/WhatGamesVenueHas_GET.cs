using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared.MachinesByLocation;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.WhatGamesVenueHas_GET
{
    public class WhatGamesVenueHas_GET
    {
        private IServiceProvider _services;

        public WhatGamesVenueHas_GET()
            : this(DI.Container.Services())
        {
        }

        public WhatGamesVenueHas_GET(IServiceProvider services)
        {
            _services = services;
            ((IArcadeMachinesByLocationRepository)_services.GetService(typeof(IArcadeMachinesByLocationRepository))).SetupTable();
        }

        public APIGatewayProxyResponse WhatGamesVenueHas_GETHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var location = request.QueryStringParameters["location"];
            if (string.IsNullOrEmpty(location))
            {
                return ErrorResponse();
            }

            var machines = ((IArcadeMachinesByLocationRepository)_services.
                GetService(typeof(IArcadeMachinesByLocationRepository))).Load(location);

            if (machines == null)
            {
                return NotFoundResponse();
            }

            return MachineResponse(machines);
        }

        private APIGatewayProxyResponse MachineResponse(ArcadeMachinesByLocation machines)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(machines),
            };
        }

        private APIGatewayProxyResponse NotFoundResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Body = "{ \"message\": \"No known games at this location.\"}",
            };
        }

        private APIGatewayProxyResponse ErrorResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"Error. No location given.\"}",
            };
        }
    }
}
