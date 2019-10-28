using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Locations;
using Arcade.Shared.Misc;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.GameDetails
{
    public class GameDetails
    {
        private IServiceProvider services;
        private IEnvironmentVariables environmentVariables;
        private IGameDetailsRepository gameDetailsRepository;
        private ILocationRepository locationRepository;
        private IObjectRepository objectRepository;
        private IMiscRepository miscRepository;

        public GameDetails()
            : this(DI.Container.Services())
        {
        }

        public GameDetails(IServiceProvider services)
        {
            this.services = services;
            environmentVariables = (IEnvironmentVariables)this.services.GetService(typeof(IEnvironmentVariables));
            gameDetailsRepository = (IGameDetailsRepository)this.services.GetService(typeof(IGameDetailsRepository));
            gameDetailsRepository.SetupTable();
            locationRepository = (ILocationRepository)this.services.GetService(typeof(ILocationRepository));
            locationRepository.SetupTable();
            objectRepository = ((IObjectRepository)this.services.GetService(typeof(IObjectRepository)));
            objectRepository.SetupTable();
            miscRepository = ((IMiscRepository)this.services.GetService(typeof(IMiscRepository)));
            miscRepository.SetupTable();
        }

        public APIGatewayProxyResponse GameDetailsHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            request.QueryStringParameters.TryGetValue("gameName", out string gameName);
            request.QueryStringParameters.TryGetValue("location", out string location);

            object response;
            switch (request.Resource)
            {
                case "/gamedetails/all":
                    if (string.IsNullOrEmpty(gameName))
                    {
                        return ErrorResponse();
                    }
                    response = GetAllInformation(gameName);
                    break;

                case "/gamedetails/availableatlocation":
                    if (string.IsNullOrEmpty(location))
                    {
                        return ErrorResponse();
                    }

                    response = GetGamesAtLocation(location);
                    break;
                case "/gamedetails/locations":

                    if (string.IsNullOrEmpty(gameName))
                    {
                        return ErrorResponse();
                    }

                    response = GetLocationsForGame(gameName);
                    break;
                case "/gamedetails/settings":

                    if (string.IsNullOrEmpty(gameName))
                    {
                        return ErrorResponse();
                    }

                    response = GetKnownSettingsForGame(gameName);
                    break;
                case "/gamedetails/rating":

                    if (string.IsNullOrEmpty(gameName))
                    {
                        return ErrorResponse();
                    }

                    response = GetRatingForGame(gameName);
                    break;
                case "/gamedetails/scores":

                    if (string.IsNullOrEmpty(gameName))
                    {
                        return ErrorResponse();
                    }

                    if (string.IsNullOrEmpty(location))
                    {
                        Console.WriteLine(gameName);
                        response = GetHighscores(gameName);
                    }
                    else
                    {
                        response = GetScoresForLocationByGame(location, gameName);
                    }
                    break;

                default:
                    response = null;
                    break;
            }

            if (response == null)
            {
                return ErrorResponse();
            }

            return Response(response);
        }

        private object GetAllInformation(string gameName)
        {
            var details = gameDetailsRepository.QueryByGameName(gameName);
            var locationDetails = details.Where(x => x.DataType.Equals("Location")).ToList();
            var ratingDetails = details.Where(x => x.DataType.Equals("Rating")).FirstOrDefault();

            var locations = GetLocationsForGame(locationDetails);
            var settings = GetKnownSettingsForGame(locationDetails);
            var scores = GetHighscores(locationDetails, gameName);
            var rating = GetRatingForGame(ratingDetails);

            var response = new {
                Locations = locations,
                Settings = settings,
                Scores = scores,
                Rating = rating };

            return Response(response);
        }

        private GameRating GetRatingForGame(GameDetailsRecord details)
        {
            if (details == null)
            {
                return new GameRating
                {
                    Average = 0,
                    NumberOfRatings = 0,
                    WeightedAverage = 0,
                };
            };

            var average = details.Average;
            var ratings = details.NumberOfRatings;
            var averageOfAllGamesRecord = miscRepository.Load("Ratings", "Average");
            var averageOfAllGames = double.Parse(averageOfAllGamesRecord.Value);

            var weightedAverage = WeightedAverageCalculator.CalculateAverage(average, (double)ratings, averageOfAllGames);

            return new GameRating
            {
                Average = details.Average,
                NumberOfRatings = details.NumberOfRatings,
                WeightedAverage = weightedAverage,
            };
        }

        private GameRating GetRatingForGame(string gameName)
        {
            var details = gameDetailsRepository.Load(gameName, "Rating");
            return GetRatingForGame(details);
        }

        private GamesAtLocation GetGamesAtLocation(string location)
        {
            var games = locationRepository.Load(location);
            return new GamesAtLocation
            {
                Games = games.GamesAvailable,
            };
        }

        private List<string> GetLocationsForGame(List<GameDetailsRecord> details)
        {
            var locations = new List<string>();            
            locations.AddRange(details.Select(x => x.SortKey));
            return locations;
        }

        private List<string> GetLocationsForGame(string gameName)
        {
            var details = gameDetailsRepository.QueryByGameName(gameName);
            return GetLocationsForGame(details.Where(x => x.DataType.Equals("Location")).ToList());
        }

        private Scores GetScoresForLocationByGame(string location, string gameName)
        {
            var details = gameDetailsRepository.Load(gameName, location);

            var scores = new Scores();
            scores.Setting = new Dictionary<string, Setting>();
            scores.SimpleScores = new Dictionary<string, List<SimpleScore>>();

            foreach (Setting setting in details.Settings)
            {
                scores.Setting.Add(setting.SettingsId, setting);

                var scoresForSetting = details.Scores.Where(x => x.SettingsId.Equals(setting.SettingsId));
                scores.SimpleScores.Add(setting.SettingsId, new List<SimpleScore>());

                foreach (ScoreDetails score in scoresForSetting)
                {
                    scores.SimpleScores[setting.SettingsId]
                        .Add(new SimpleScore { UserName = score.UserName, Score = score.Score });
                }
            }

            return scores;
        }

        private List<Setting> GetKnownSettingsForGame(List<GameDetailsRecord> details)
        {
            var settings = new List<Setting>();
            foreach (GameDetailsRecord detail in details)
            {
                settings.AddRange(detail.Settings);
            }
            var uniqueSettings = settings.Distinct();
            return uniqueSettings.ToList();
        }

        private List<Setting> GetKnownSettingsForGame(string gameName)
        {
            var details = gameDetailsRepository.QueryByGameName(gameName);
            return GetKnownSettingsForGame(details.Where(x => x.DataType.Equals("Location")).ToList());
        }

        private Scores GetHighscores(string gameName)
        {
            var details = gameDetailsRepository.QueryByGameName(gameName);
            return GetHighscores(details.Where(x => x.DataType.Equals("Location")).ToList(), gameName);
        }

        private Scores GetHighscores(List<GameDetailsRecord> details, string gameName)
        {
            //Need to go over each details object in here as the outer loop
            var scores = new Scores();
            scores.Setting = new Dictionary<string, Setting>();
            scores.SimpleScores = new Dictionary<string, List<SimpleScore>>();

            List<Setting> settingsFound = new List<Setting>();
            List<SimpleScore> allScores = new List<SimpleScore>();

            foreach (GameDetailsRecord detail in details)
            {
                foreach (Setting setting in detail.Settings)
                {
                    var settingIndex = -1;

                    if(!settingsFound.Contains(setting))
                    {
                        settingsFound.Add(setting);
                        settingIndex = settingsFound.IndexOf(setting);
                        scores.Setting.Add(settingIndex.ToString(), setting);
                    }
                    else
                    {
                        settingIndex = settingsFound.IndexOf(setting);
                    }

                    var scoresForSetting = detail.Scores.Where(x => x.SettingsId.Equals(setting.SettingsId));

                    if (!scores.SimpleScores.ContainsKey(settingIndex.ToString()))
                    {
                        scores.SimpleScores.Add(settingIndex.ToString(), new List<SimpleScore>());
                    }                    

                    foreach (ScoreDetails score in scoresForSetting)
                    {
                        scores.SimpleScores[settingIndex.ToString()]
                            .Add(new SimpleScore { UserName = score.UserName, Score = score.Score });

                        allScores.Add(new SimpleScore { UserName = score.UserName, Score = score.Score });
                    }
                }
            }

            //Create final list of all scores submitted
            //In teh detailed scoreboards
            scores.Setting.Add("ALL", new Setting());
            scores.SimpleScores.Add("ALL", allScores);
            //Need to add all the scores from the standard scores too...
            var gamescore = objectRepository.Load(gameName);
            if (gamescore != null)
            {
                foreach(KeyValuePair<string,string> score in gamescore.DictionaryValue)
                {
                    //Need to get the pairs of simple scores from this repository
                    scores.SimpleScores["ALL"].Add(new SimpleScore
                    {
                        UserName = score.Key,
                        Score = score.Value,
                    });
                }
            }

            return scores;
        }

        private APIGatewayProxyResponse Response(object returnObject)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(returnObject),
            };
        }

        private APIGatewayProxyResponse ErrorResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"Error getting game details\" }",
            };
        }
    }
}
