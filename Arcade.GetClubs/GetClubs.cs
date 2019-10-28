using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Misc;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.GetClubs
{
    public class GetClubs
    {
        private IServiceProvider services;

        public GetClubs()
            : this(DI.Container.Services())
        {
        }

        public GetClubs(IServiceProvider services)
        {
            this.services = services;
            ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).SetupTable();
            ((IClubRepository)this.services.GetService(typeof(IClubRepository))).SetupTable();
        }

        public APIGatewayProxyResponse GetClubsHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var clubNames = ((IMiscRepository)services.GetService(typeof(IMiscRepository)))
                    .Load("Clubs", "Order");

                var clubsInformation = new List<ClubInformation>();
                var clubRepository = (IClubRepository)services.GetService(typeof(IClubRepository));

                foreach (string name in clubNames.List1)
                {
                    var information = clubRepository.Load(name);
                    if (information != null)
                    {
                        if (!string.IsNullOrWhiteSpace(information.Secret))
                        {
                            information.Secret = "Protected";
                        }
                        else
                        {
                            information.Secret = string.Empty;
                        }

                        clubsInformation.Add(information);
                    }
                }

                return OkResponse(clubsInformation);
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        private APIGatewayProxyResponse OkResponse(List<ClubInformation> clubs)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(clubs),
            };
        }

        private APIGatewayProxyResponse ErrorResponse(string error)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = "{ \"message\": \"Error. " + error + "\"}",
            };
        }
    }
}
