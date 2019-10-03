using System;
using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.GetRating
{
    public class GetRating
    {
        private IServiceProvider services;

        public GetRating()
            : this(DI.Container.Services())
        {
        }

        public GetRating(IServiceProvider services)
        {
            this.services = services;
            ((IRatingRepository)this.services.GetService(typeof(IRatingRepository))).SetupTable();
        }

        public APIGatewayProxyResponse GetRatingHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            request.QueryStringParameters.TryGetValue("gamename", out string gamename);

            if (string.IsNullOrEmpty(gamename))
            {
                return ErrorResponse();
            }

            var ratingInfo = GetRatingInfo(gamename);
            if (ratingInfo == null)
            {
                ratingInfo = new RatingInformation
                {
                    GameName = gamename,
                    Average = 0,
                    NumberOfRatings = 0,
                };
            }

            return Response(ratingInfo);
        }

        private RatingInformation GetRatingInfo(string gamename)
        {
            var ratingInformationForGame = ((IRatingRepository)services.GetService(typeof(IRatingRepository))).Load(gamename);

            return ratingInformationForGame;
        }

        private APIGatewayProxyResponse Response(RatingInformation ratingInfo)
        {
            var response = new SaveRatingInformationResponse();
            response.Games = new Dictionary<string, SingleRatingInformationResponse>();
            response.Games.Add(ratingInfo.GameName, new SingleRatingInformationResponse
            {
                Average = ratingInfo.Average,
                NumberOfRatings = ratingInfo.NumberOfRatings,
            });

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(response),
            };
        }

        private APIGatewayProxyResponse ErrorResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"Error no game supplied\" }",
            };
        }
    }
}
