using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared.ListItems;
using Arcade.Shared.Locations;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.Locations_GET
{
    public class Locations_GET
    {

        private IServiceProvider _services;

        public Locations_GET()
            : this(DI.Container.Services())
        {
        }

        public Locations_GET(IServiceProvider services)
        {
            _services = services;
            ((IListItemsRepository)_services.GetService(typeof(IListItemsRepository))).SetupTable();
            ((ILocationRepository)_services.GetService(typeof(ILocationRepository))).SetupTable();
        }

        public APIGatewayProxyResponse Locations_GETHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var locationsFromList = ((IListItemsRepository)_services.
                GetService(typeof(IListItemsRepository))).Load("Locations");

            var locations = new List<Location>();

            var locationTable = (ILocationRepository)_services.GetService(typeof(ILocationRepository));

            foreach (string location in locationsFromList.Items)
            {
                var loc = locationTable.Load(location);
                if(loc != null)
                {
                    locations.Add(loc);
                }
            }

            return LocationsResponse(locations);
        }

        private APIGatewayProxyResponse LocationsResponse(List<Location> locations)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(locations),
            };
        }                
    }
}
