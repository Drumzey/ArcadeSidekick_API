using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared.MachinesByLocation;
using Arcade.Shared.Shared;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.WhatGamesVenueHas_POST
{
    public class WhatGamesVenueHas_POST
    {
        private IServiceProvider _services;

        public WhatGamesVenueHas_POST()
            : this(DI.Container.Services())
        {
        }

        public WhatGamesVenueHas_POST(IServiceProvider services)
        {
            _services = services;
            ((IArcadeMachinesByLocationRepository)_services.GetService(typeof(IArcadeMachinesByLocationRepository))).SetupTable();
        }

        public APIGatewayProxyResponse WhatGamesVenueHas_GETHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var data = JsonConvert.DeserializeObject<ArcadeMachinesByLocationInput>(request.Body);

            if(string.IsNullOrEmpty(data.Location))
            {
                return ErrorResponse("No location supplied");
            }

            if (string.IsNullOrEmpty(data.GameName))
            {
                return ErrorResponse("No game supplied");
            }

            var service = (IArcadeMachinesByLocationRepository)_services.
                GetService(typeof(IArcadeMachinesByLocationRepository));
            var games = service.Load(data.Location);
                        
            if (games == null)
            {
                games = new ArcadeMachinesByLocation
                {
                    Location = data.Location,
                    Machines = new Dictionary<string, List<Setting>>(),                    
                };

                games.Machines.Add(data.GameName, new List<Setting> { data.Setting });

                return MachineResponse(games);
            }

            if (!games.Machines.ContainsKey(data.GameName))
            {
                games.Machines.Add(data.GameName, new List<Setting>
                {
                    data.Setting,
                });

                return MachineResponse(games);
            }

            var found = false;
            //Do we already contain this setting for this machine.
            foreach(Setting setting in games.Machines[data.GameName])
            {
                if(setting.Difficulty == data.Setting.Difficulty &&
                    setting.ExtraLivesAt == data.Setting.ExtraLivesAt &&
                    setting.LevelName == data.Setting.LevelName &&
                    setting.Lives == data.Setting.Lives)
                {
                    found = true;
                }
            }

            if (found == false)
            {
                games.Machines[data.GameName].Add(data.Setting);
            }

            return MachineResponse(games);
        }

        private APIGatewayProxyResponse MachineResponse(ArcadeMachinesByLocation games)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(games),
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
