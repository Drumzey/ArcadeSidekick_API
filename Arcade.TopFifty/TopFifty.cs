using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
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
            ((IObjectRepository)this.services.GetService(typeof(IObjectRepository))).SetupTable();
        }

        public APIGatewayProxyResponse TopFiftyHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var ratings = ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Load("ratings");
                var topFifty = GetTop50Ratings(ratings.DictionaryValue);
                return Response(topFifty);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return ErrorResponse();
        }

        private List<Game> GetTop50Ratings(Dictionary<string, string> dictionaryValue)
        {
            var numbericVersions = dictionaryValue.ToDictionary(g => g.Key, g => Convert.ToDouble(g.Value));

            List<Game> orderedGames = new List<Game>();

            foreach (KeyValuePair<string, double> game in numbericVersions.OrderByDescending(game => game.Value))
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
