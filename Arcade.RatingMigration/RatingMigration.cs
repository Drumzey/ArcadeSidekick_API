using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.GameDetails;
using Arcade.Shared;
using Arcade.Shared.Misc;
using Arcade.Shared.Repositories;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.RatingMigration
{
    public class RatingMigration
    {

        private IServiceProvider _services;

        public RatingMigration()
            : this(DI.Container.Services())
        {
        }

        public RatingMigration(IServiceProvider services)
        {
            _services = services;
            ((IRatingRepository)_services.GetService(typeof(IRatingRepository))).SetupTable();
            ((IGameDetailsRepository)_services.GetService(typeof(IGameDetailsRepository))).SetupTable();
            ((IMiscRepository)_services.GetService(typeof(IMiscRepository))).SetupTable();
        }

        public APIGatewayProxyResponse RatingMigrationHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var totalNumberOfRatings = 0;
            var totalOfRatings = 0;

            var ratings = ((IRatingRepository)_services.GetService(typeof(IRatingRepository))).AllRows();

            foreach (RatingInformation rating in ratings)
            {
                var gameDetailsRow = ((IGameDetailsRepository)_services.GetService(typeof(IGameDetailsRepository))).Load(rating.GameName, "Rating");

                if (gameDetailsRow == null)
                {
                    gameDetailsRow = new GameDetailsRecord();
                    gameDetailsRow.Game = rating.GameName;
                    gameDetailsRow.SortKey = "Rating";
                    gameDetailsRow.DataType = "Rating";
                }

                gameDetailsRow.Average = rating.Average;
                gameDetailsRow.CreatedAt = rating.CreatedAt;
                gameDetailsRow.NumberOfRatings = rating.NumberOfRatings;
                gameDetailsRow.Ratings = rating.Ratings;
                gameDetailsRow.Total = rating.Total;
                gameDetailsRow.UpdatedAt = rating.UpdatedAt;

                totalNumberOfRatings += rating.NumberOfRatings;
                totalOfRatings += rating.Total;

                ((IGameDetailsRepository)_services.GetService(typeof(IGameDetailsRepository))).Save(gameDetailsRow);
            }

            var miscRepository = (IMiscRepository)_services.GetService(typeof(IMiscRepository));
            var totalNumber = miscRepository.Load("Ratings", "TotalNumber");
            var total = miscRepository.Load("Ratings", "Total");
            var averageRating = miscRepository.Load("Ratings", "Average");

            totalNumber.Value = totalNumberOfRatings.ToString();
            total.Value = totalOfRatings.ToString();

            averageRating.Value = Math.Round(double.Parse(total.Value) / double.Parse(totalNumber.Value), 2).ToString();

            miscRepository.Save(totalNumber);
            miscRepository.Save(total);
            miscRepository.Save(averageRating);

            return Response();
        }

        private APIGatewayProxyResponse Response()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "{ \"message\": \"Done\"}",
            };
        }

    }
}
