using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Repositories;
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
            ((IObjectRepository)this.services.GetService(typeof(IObjectRepository))).SetupTable();
        }

        public APIGatewayProxyResponse RecentActivityHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var recent = ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Load("recent");
                return Response(recent.ListValue);
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
