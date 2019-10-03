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

namespace Arcade.WhereCanIPlay_POST
{       
    public class WhereCanIPlay_POST
    {
        private IServiceProvider _services;

        public WhereCanIPlay_POST()
            : this(DI.Container.Services())
        {
        }

        public WhereCanIPlay_POST(IServiceProvider services)
        {
            _services = services;
            ((ILocationByMachineRepository)_services.GetService(typeof(ILocationByMachineRepository))).SetupTable();
        }

        public APIGatewayProxyResponse WhereCanIPlay_POSTHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var data = JsonConvert.DeserializeObject<LocationByMachineInput>(request.Body);

            var service = (ILocationByMachineRepository)_services.
                GetService(typeof(ILocationByMachineRepository));
            var locations = service.Load(data.GameName);

            if (locations == null)
            {
                locations = new LocationByMachine
                {
                    GameName = data.GameName,
                    Locations = new List<string>
                    {
                        data.Location,
                    },
                };

                service.Save(locations);

                return LocationResponse(locations);
            }
            
            //Add location if not already present
            if(!locations.Locations.Contains(data.Location))
            {
                locations.Locations.Add(data.Location);
                service.Save(locations);
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
                Body = "{ \"message\": \"Error. Cannot get locations for game.\"}",
            };
        }
    }
}
