using System;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared.SettingsByGameName;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.Settings_GET
{
    public class Settings_GET
    {

        private IServiceProvider _services;

        public Settings_GET()
            : this(DI.Container.Services())
        {
        }

        public Settings_GET(IServiceProvider services)
        {
            _services = services;
            ((ISettingsByGameNameRepository)_services.GetService(typeof(ISettingsByGameNameRepository))).SetupTable();
        }

        public APIGatewayProxyResponse Settings_GETHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var gameName = request.QueryStringParameters["gamename"];
            if (string.IsNullOrEmpty(gameName))
            {
                return ErrorResponse();
            }

            var settings = ((ISettingsByGameNameRepository)_services.
                GetService(typeof(ISettingsByGameNameRepository))).Load(gameName);

            if(settings == null)
            {
                return NotFoundResponse();
            }

            return SettingsResponse(settings);
        }

        private APIGatewayProxyResponse SettingsResponse(SettingsByGameName settings)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(settings),
            };
        }

        private APIGatewayProxyResponse NotFoundResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Body = "{ \"message\": \"No settings found for this game.\"}",
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
