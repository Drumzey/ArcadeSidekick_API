using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Misc;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.TopFifty
{
    internal class Game
    {
        public string Name { get; set; }

        public double Rating { get; set; }
    }

    public class TopFifty
    {
        private readonly IEnvironmentVariables environmentVariables;
        private IServiceProvider services;

        public TopFifty()
            : this(DI.Container.Services())
        {
        }

        public TopFifty(IServiceProvider services)
        {
            this.services = services;
            environmentVariables = (IEnvironmentVariables)this.services.GetService(typeof(IEnvironmentVariables));            
            ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).SetupTable();
        }

        public APIGatewayProxyResponse TopFiftyHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var arcadeRatings = ((IMiscRepository)services.GetService(typeof(IMiscRepository)))
                    .Load("Ratings", "Arcade Games");
                var pinballRatings = ((IMiscRepository)services.GetService(typeof(IMiscRepository)))
                    .Load("Ratings", "Pinball");
                var averageOfAllVotes = ((IMiscRepository)services.GetService(typeof(IMiscRepository)))
                    .Load("Ratings", "Average");

                var topArcades = GetTop50WeightedAverage(arcadeRatings.Dictionary, averageOfAllVotes.Value);
                var topPinball = GetTop50WeightedAverage(pinballRatings.Dictionary, averageOfAllVotes.Value);

                return Response(topArcades);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return ErrorResponse();
        }

        private List<Game> GetTop50WeightedAverage(Dictionary<string, string> dictionaryValue, string mean)
        {
            var numericVersions = dictionaryValue.ToDictionary(
                g => g.Key,
                g => WeightedAverageCalculator.CalculateAverage(
                    double.Parse(g.Value.Split(',')[0]),
                    double.Parse(g.Value.Split(',')[1]),
                    double.Parse(mean)));

            List<Game> orderedGames = new List<Game>();

            foreach (KeyValuePair<string, double> game in numericVersions.OrderByDescending(game => game.Value))
            {
                Game g = new Game();
                g.Name = game.Key;
                g.Rating = Math.Round(game.Value, 2);
                orderedGames.Add(g);
            }

            return orderedGames.Take(50).ToList();
        }

        private APIGatewayProxyResponse Response(List<Game> ratingInfo)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(ratingInfo),
            };
        }

        private APIGatewayProxyResponse ErrorResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"Error getting top fifty\" }",
            };
        }
    }
}
