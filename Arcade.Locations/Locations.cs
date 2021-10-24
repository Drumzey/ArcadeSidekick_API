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

        private List<string> countryOrder = new List<string>
        {
            "UK", "IRE", "USA", "AUS", "CAN", "FRA"
        };

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

                case "/app/venues/join":
                    response = JoinPrivateVenue(request);
                    break;

                case "/website/venues":
                    response = GetLocation(request);
                    break;

                default:
                    return ErrorResponse();
            }

            return Response(response);
        }

        private APIGatewayProxyResponse JoinPrivateVenue(APIGatewayProxyRequest request)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<LocationMembership>(request.Body);

                if (string.IsNullOrEmpty(data.VenueName))
                {
                    return ErrorResponse("No venue name given");
                }

                if (string.IsNullOrEmpty(data.Password))
                {
                    return ErrorResponse("No password given");
                }

                var venue = locationRepository.Load(data.VenueName);

                if (venue == null || venue.Private == false)
                {
                    return ErrorResponse("Incorrect venue details");
                }

                if (venue.Password != data.Password)
                {
                    return ErrorResponse("Incorrect venue details");
                }

                return Response(true);
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        private APIGatewayProxyResponse GetLocation(APIGatewayProxyRequest request)
        {
            try
            {
                request.QueryStringParameters.TryGetValue("password", out string password);
                request.QueryStringParameters.TryGetValue("location", out string location);

                if (string.IsNullOrEmpty(location))
                {
                    return ErrorResponse("No venue name given");
                }

                var venue = locationRepository.Load(location);

                if (venue == null)
                {
                    return ErrorResponse("Incorrect venue details");
                }

                if (venue.Password != password)
                {
                    return ErrorResponse("Incorrect venue details");
                }

                return Response(venue);
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        public List<Location> GetLocations()
        {
            var locations = locationRepository.AllLocations();

            foreach(var location in locations)
            {
                //Blank out the password
                location.Password = "";
            }

            //Add the locations in country sections, ordered alphabetically
            var orderedLocations = new List<Location>();
            foreach(string country in countryOrder)
            {
                orderedLocations.AddRange(locations.Where(x => x.Country.Equals(country)).OrderBy(x => x.Name));
            }

            //Add all locations whose country is not defined in the list
            orderedLocations.AddRange(locations.Where(x => !countryOrder.Contains(x.Country)).OrderBy(x => x.Name));

            return orderedLocations;
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

        private APIGatewayProxyResponse ErrorResponse(string message)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"" + message + "\" }",
            };
        }
    }
}
