using System;
using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Misc;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.RecentActivity
{
    public class RecentActivity
    {
        private readonly IEnvironmentVariables environmentVariables;
        private IServiceProvider services;

        public RecentActivity()
            : this(DI.Container.Services())
        {
        }

        public RecentActivity(IServiceProvider services)
        {
            this.services = services;
            environmentVariables = (IEnvironmentVariables)this.services.GetService(typeof(IEnvironmentVariables));
            ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).SetupTable();
        }

        public APIGatewayProxyResponse RecentActivityHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var recentMisc = ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Load("Activity", "All");
                return Response(recentMisc.List1);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return ErrorResponse();
        }

        private APIGatewayProxyResponse Response(List<string> recent)
        {
            recent.Reverse();
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(recent),
            };
        }

        private APIGatewayProxyResponse ErrorResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"Error getting recent activity\" }",
            };
        }
    }
}
