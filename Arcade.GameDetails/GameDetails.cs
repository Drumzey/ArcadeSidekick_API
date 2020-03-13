// This lambda deals with getting all information about games
//
// GET - Rating
// GET - Settings
// GET - What games are available at which location
// GET - Where is this game available
// GET - Scores
// NEED TO ADD - TOP 50
//
// GET ALL - This call will get the following information about a game in a single call
//   Rating
//   Settings
//   Scores
//   Locations
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.GameDetails.Handlers;
using Arcade.Shared;
using Arcade.Shared.Locations;
using Arcade.Shared.Misc;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;

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
            //If we are posting something to the gamedetails then we are performing 
            //a set of a new score
            if (request.HttpMethod == "POST")
            {
                return SetHandler(request);
            }

            //If we are deleting something in the gamedetails then we are performing 
            //a delete of an already existing score
            if (request.HttpMethod == "DELETE")
            {
                return DeleteHandler(request);
            }

            request.QueryStringParameters.TryGetValue("gameName", out string gameName);
            request.QueryStringParameters.TryGetValue("userName", out string userName);
            request.QueryStringParameters.TryGetValue("location", out string location);

            switch (request.Resource)
            {
                case "/app/games/all":
                    if (string.IsNullOrEmpty(gameName))
                    {
                        return ErrorResponse();
                    }
                    if (string.IsNullOrEmpty(userName))
                    {
                        return ErrorResponse();
                    }
                    break;                
                case "/app/games/knownsettings":
                case "/app/games/ratingsweighted":
                case "/app/games/detailedscore":
                case "/app/games/knownlevels":
                    if (string.IsNullOrEmpty(gameName))
                    {
                        return ErrorResponse();
                    }
                    break;

                // case "/gamedetails/stats": NOT SURE WHAT THIS IS?
                //if (string.IsNullOrEmpty(gameName))
                //{
                //    return ErrorResponse();
                //}
                //if (string.IsNullOrEmpty(userName))
                //{
                //    return ErrorResponse();
                //}

                //case "/gamedetails/locations":
                //if (string.IsNullOrEmpty(gameName))
                //{
                //    return ErrorResponse();
                //}
                //break;

                //case "/app/games/availableat":
                //    if (string.IsNullOrEmpty(location))
                //    {
                //        return ErrorResponse();
                //    }
                //    break;
                default:
                    break;
            }


            object response;
            switch (request.Resource)
            {
                case "/app/games/all":
                    response = GetAllInformation(gameName, userName);
                    break;

                case "/app/games/knownsettings":
                    response = GetKnownSettingsForGame(gameName);
                    break;

                case "/app/games/ratingsweighted":
                    response = GetRatingForGame(gameName);
                    break;

                case "/app/games/knownlevels":
                    response = GetLevelsForGame(gameName);
                    break;

                case "/app/games/detailedscore":
                    if (string.IsNullOrEmpty(location))
                    {
                        response = GetAllHighscores(gameName);
                    }
                    else
                    {
                        response = GetScoresForLocationByGame(location, gameName);
                    }
                    break;

                //case "/app/games/availableat":
                //    response = GetGamesAtLocation(location);
                //    break;

                //case "/gamedetails/locations":
                //    response = GetLocationsForGame(gameName);
                //    break;

                //case "/gamedetails/stats":
                //    response = GetUserStatsForGame(userName, gameName);
                //    break;

                default:
                    return ErrorResponse();
            }

            return Response(response);
        }

        private APIGatewayProxyResponse DeleteHandler(APIGatewayProxyRequest request)
        {
            switch (request.Resource)
            {
                case "/app/games/detailedscore":
                    DeleteHighscore(request.Body);
                    return OKResponse();
            }

            return ErrorResponse();
        }

        private APIGatewayProxyResponse SetHandler(APIGatewayProxyRequest request)
        {
            switch (request.Resource)
            {
                case "/app/games/detailedscore":
                     SetHighscore(request.Body);
                     return OKResponse();
            }

            return ErrorResponse();
        }

        private object GetAllInformation(string gameName, string userName)
        {
            var details = gameDetailsRepository.QueryByGameName(gameName);
            var locationDetails = details.Where(x => x.DataType.Equals("Location")).ToList();
            var ratingDetails = details.Where(x => x.DataType.Equals("Rating")).FirstOrDefault();
            var settingsDetails = details.Where(x => x.SortKey.Equals("Settings")).FirstOrDefault();
            var statsDetails = details.Where(x => x.SortKey.Equals(userName)).FirstOrDefault();

            var locations = GetLocationsForGame(locationDetails);
            var settings = GetKnownSettingsForGame(settingsDetails);
            var scores = GetAllHighscores(locationDetails, gameName);
            var rating = GetRatingForGame(ratingDetails);
            var stats = GetUserStatsForGame(statsDetails, settingsDetails);

            var response = new {
                Locations = locations,
                Settings = settings,
                Scores = scores,
                Rating = rating,
                History = stats };

            return Response(response);
        }

        private LevelsResponse GetLevelsForGame(string gameName)
        {
            var levels = gameDetailsRepository.Load(gameName, "Levels");

            if (levels != null)
            {
                return new LevelsResponse{
                    Levels = levels.Levels,
                    Finalised = levels.Finalised,
                };
            }

            return new LevelsResponse
            {
                Levels = new List<string>(),
                Finalised = false,
            };
        }

        private GamesAtLocation GetGamesAtLocation(string location)
        {
            var games = locationRepository.Load(location);
            return new GamesAtLocation
            {
                Games = games.GamesAvailable,
            };
        }

        private GameDetailsRecord GetUserStatsForGame(string userName, string gameName)
        {
            var handler = new StatsHandler();
            return handler.Get(gameDetailsRepository, gameName, userName);
        }

        private GameDetailsRecord GetUserStatsForGame(GameDetailsRecord details, GameDetailsRecord stats)
        {
            details.Settings = stats.Settings;
            return details;
        }

        #region RATING
        private GameRating GetRatingForGame(GameDetailsRecord details)
        {
            var handler = new RatingHandler();
            return handler.Get(miscRepository, details);
        }

        private GameRating GetRatingForGame(string gameName)
        {
            var handler = new RatingHandler();
            return handler.Get(gameDetailsRepository, miscRepository, gameName);
        }
        #endregion

        #region LOCATIONS
        private List<string> GetLocationsForGame(List<GameDetailsRecord> details)
        {
            LocationHandler handler = new LocationHandler();
            return handler.Get(details);
        }

        private List<string> GetLocationsForGame(string gameName)
        {
            LocationHandler handler = new LocationHandler();
            return handler.Get(gameDetailsRepository, gameName);
        }
        #endregion

        #region SETTINGS
        private List<Setting> GetKnownSettingsForGame(GameDetailsRecord details)
        {
            var handler = new SettingsHandler();
            return handler.Get(details);
        }

        private List<Setting> GetKnownSettingsForGame(string gameName)
        {
            var handler = new SettingsHandler();
            return handler.Get(gameDetailsRepository, gameName);
        }
        #endregion

        #region HIGHSCORES

        private void DeleteHighscore(string body)
        {
            var handler = new HighScoreHandler();
            handler.Delete(gameDetailsRepository, locationRepository, body);
        }

        private void SetHighscore(string body)
        {
            var handler = new HighScoreHandler();
            handler.Set(gameDetailsRepository, locationRepository, body);
        }

        private Scores GetAllHighscores(string gameName)
        {
            var handler = new HighScoreHandler();
            return handler.GetAll(gameDetailsRepository, objectRepository, gameName);
        }

        private Scores GetAllHighscores(List<GameDetailsRecord> details, string gameName)
        {
            var handler = new HighScoreHandler();
            return handler.GetAll(gameDetailsRepository, objectRepository, details, gameName);
        }

        private Scores GetScoresForLocationByGame(string location, string gameName)
        {
            var handler = new HighScoreHandler();
            return handler.GetAllByLocation(gameDetailsRepository, gameName, location);
        }
        #endregion

        private APIGatewayProxyResponse OKResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "{ \"message\": \"Score added.\"}",
            };
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

    class LevelsResponse
    {
        public List<string> Levels { get; set; }
        public bool Finalised { get; set; }
    }
}
