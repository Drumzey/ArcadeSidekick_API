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

namespace Arcade.SaveRating
{
    public class SaveRating
    {
        private IServiceProvider _services;

        public SaveRating()
            : this(DI.Container.Services())
        {
        }

        public SaveRating(IServiceProvider services)
        {
            _services = services;
            ((IRatingRepository)_services.GetService(typeof(IRatingRepository))).SetupTable();
        }

        public APIGatewayProxyResponse SaveRatingHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var ratingInfo = SaveRatingInfo(request.Body);
            return Response(ratingInfo);
        }

        private RatingInformation SaveRatingInfo(string requestBody)
        {
            var data = JsonConvert.DeserializeObject<SaveRatingInformationInput>(requestBody);

            var ratingInformationForGame = ((IRatingRepository)_services.GetService(typeof(IRatingRepository))).Load(data.GameName);

            if (ratingInformationForGame == null)
            {
                ratingInformationForGame = new RatingInformation
                {
                    GameName = data.GameName,
                    NumberOfRatings = 1,
                    Average = data.Rating,
                    Total = data.Rating,
                    CreatedAt = DateTime.Now,
                    Ratings = new Dictionary<string, int>
                    {
                        { data.Username, data.Rating},
                    }
                };
            }
            else
            {
                if (ratingInformationForGame.Ratings.ContainsKey(data.Username))
                {
                    var oldRating = ratingInformationForGame.Ratings[data.Username];
                    var newRating = data.Rating;

                    if (oldRating != newRating)
                    {
                        ratingInformationForGame.Ratings[data.Username] = newRating;
                        ratingInformationForGame.Total += (newRating - oldRating);
                    }
                }
                else
                {
                    ratingInformationForGame.Ratings.Add(data.Username, data.Rating);
                    ratingInformationForGame.NumberOfRatings++;
                    ratingInformationForGame.Total += data.Rating;
                }

                ratingInformationForGame.Average = ratingInformationForGame.Total / ratingInformationForGame.NumberOfRatings;
            }

            ratingInformationForGame.UpdatedAt = DateTime.Now;
            ((IRatingRepository)_services.GetService(typeof(IRatingRepository))).Save(ratingInformationForGame);

            return ratingInformationForGame;
        }

        private APIGatewayProxyResponse Response(RatingInformation ratingInfo)
        {
            var response = new RatingInformationResponse
            {
                Average = ratingInfo.Average,
                NumberOfRatings = ratingInfo.NumberOfRatings,
            };

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(response),
            };
        }
    }
}
