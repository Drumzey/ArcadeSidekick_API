using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.OvernightLeaderboard
{
    public class OvernightLeaderboard
    {
        private IServiceProvider _services;

        public OvernightLeaderboard()
            : this(DI.Container.Services())
        {
        }

        public OvernightLeaderboard(IServiceProvider services)
        {
            _services = services;
            ((IObjectRepository)_services.GetService(typeof(IObjectRepository))).SetupTable();
        }

        public APIGatewayProxyResponse OvernightLeaderboardHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            //Get all leaderboards

            //Write to a json file on s3


            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }

    }
}
