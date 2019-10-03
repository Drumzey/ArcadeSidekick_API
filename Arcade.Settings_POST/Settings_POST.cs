using System;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared.SettingsByGameName;
using Arcade.Shared.Shared;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.Settings_POST
{
    public class Settings_POST
    {

        private IServiceProvider _services;

        public Settings_POST()
            : this(DI.Container.Services())
        {
        }

        public Settings_POST(IServiceProvider services)
        {
            _services = services;
            ((ISettingsByGameNameRepository)_services.GetService(typeof(ISettingsByGameNameRepository))).SetupTable();
        }

        public APIGatewayProxyResponse Settings_GETHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var data = JsonConvert.DeserializeObject<SettingInput>(request.Body);

            var error = ValidateInput(data);
            if (error != null)
            {
                return error;
            }

            var settingsService = (ISettingsByGameNameRepository)_services.
                GetService(typeof(ISettingsByGameNameRepository));

            //Does this setting already exist?
            var settings = settingsService.Load(data.GameName);

            var found = false;
            foreach (Setting setting in settings.Settings)
            {
                if (setting.Difficulty == data.Difficulty &&
                setting.ExtraLivesAt == data.ExtraLivesAt &&
                setting.LevelName == data.LevelName &&
                setting.Lives == data.Lives &&
                setting.MameOrPCB == data.MameOrPCB)
                {
                    found = true;
                    break;
                }
            }

            if(found)
            {
                return ErrorResponse("Setting already exists");
            }

            settings.Settings.Add(new Setting
            {
                LevelName = data.LevelName,
                Difficulty = data.Difficulty,
                ExtraLivesAt = data.ExtraLivesAt,
                Lives = data.Lives,
                MameOrPCB = data.MameOrPCB,
            });

            settingsService.Save(settings);
            return SettingsResponse(settings);
        }

        public APIGatewayProxyResponse ValidateInput(SettingInput data)
        {
            if (string.IsNullOrEmpty(data.GameName))
            {
                return ErrorResponse("No Game Name");
            }

            if (string.IsNullOrEmpty(data.Difficulty) &&
                data.ExtraLivesAt == 0 &&
                string.IsNullOrEmpty(data.LevelName) &&
                data.Lives == 0 &&
                string.IsNullOrEmpty(data.MameOrPCB))
            {
                return ErrorResponse("No details given");
            }

            return null;
        }

        private APIGatewayProxyResponse SettingsResponse(SettingsByGameName settings)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(settings),
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
