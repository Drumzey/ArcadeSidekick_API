using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.GameDetails;
using Arcade.Shared.Misc;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.AverageMigration
{
    public class AverageMigration
    {
        private IServiceProvider _services;

        public AverageMigration()
            : this(DI.Container.Services())
        {
        }

        public AverageMigration(IServiceProvider services)
        {
            _services = services;            
            ((IGameDetailsRepository)_services.GetService(typeof(IGameDetailsRepository))).SetupTable();
            ((IMiscRepository)_services.GetService(typeof(IMiscRepository))).SetupTable();
        }

        private string ConvertName(string name)
        {
            name = name.ToLower();
            name = name.Replace(" ", "_");
            return name;
        }

        public APIGatewayProxyResponse AverageMigrationHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var ratings = ((IGameDetailsRepository)_services.GetService(typeof(IGameDetailsRepository))).AllRows();
            ratings = ratings.Where(x => x.SortKey.Equals("Rating"));

            Console.WriteLine(ratings);

            var pinballGames = ((IMiscRepository)_services.GetService(typeof(IMiscRepository)))
                    .Load("Games", "Pinball");
            var pinballNames = pinballGames.List1.ConvertAll(g => ConvertName(g));
            pinballNames.AddRange(pinballGames.List2.ConvertAll(g => ConvertName(g)));

            Console.WriteLine(pinballNames);

            var pinballRatings = ((IMiscRepository)_services.GetService(typeof(IMiscRepository)))
                    .Load("Ratings", "Pinball");
            var arcadeRatings = ((IMiscRepository)_services.GetService(typeof(IMiscRepository)))
                    .Load("Ratings", "Arcade Games");

            foreach (GameDetailsRecord rating in ratings)
            {
                if (pinballNames.Contains(rating.Game))
                {
                    if (pinballRatings.Dictionary == null)
                    {
                        pinballRatings.Dictionary = new Dictionary<string, string>();
                    }

                    if (pinballRatings.Dictionary.ContainsKey(rating.Game))
                    {
                        pinballRatings.Dictionary[rating.Game] = rating.Average.ToString() + "," + rating.NumberOfRatings.ToString();
                    }
                    else
                    {
                        pinballRatings.Dictionary.Add(rating.Game, rating.Average.ToString() + "," + rating.NumberOfRatings.ToString());
                    }
                }
                else
                {
                    if (arcadeRatings.Dictionary == null)
                    {
                        arcadeRatings.Dictionary = new Dictionary<string, string>();
                    }

                    if (arcadeRatings.Dictionary.ContainsKey(rating.Game))
                    {
                        arcadeRatings.Dictionary[rating.Game] = rating.Average.ToString() + "," + rating.NumberOfRatings.ToString();
                    }
                    else
                    {
                        arcadeRatings.Dictionary.Add(rating.Game, rating.Average.ToString() + "," + rating.NumberOfRatings.ToString());
                    }
                }
            }
            
            ((IMiscRepository)_services.GetService(typeof(IMiscRepository))).Save(pinballRatings);
            ((IMiscRepository)_services.GetService(typeof(IMiscRepository))).Save(arcadeRatings);

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
