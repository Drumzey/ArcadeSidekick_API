using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.GameDetails;
using Arcade.GetLeaderboard;
using Arcade.Shared.Leagues;
using Arcade.Shared.Misc;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.GetLeagueScores
{
    public class GetLeagueScores
    {
        private IServiceProvider services;

        public GetLeagueScores()
            : this(DI.Container.Services())
        {
            Console.WriteLine("In constructor");
        }

        public GetLeagueScores(IServiceProvider services)
        {
            Console.WriteLine("In constructor");
            this.services = services;
            ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).SetupTable();
            ((IClubRepository)this.services.GetService(typeof(IClubRepository))).SetupTable();
            ((ILeagueRepository)this.services.GetService(typeof(ILeagueRepository))).SetupTable();
        }

        public APIGatewayProxyResponse GetLeagueScoresHandler(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<GetLeagueScoresRequest>(request.Body);

                var leagueRepo = (ILeagueRepository)services.GetService(typeof(ILeagueRepository));
                var league = leagueRepo.Load(data.LeagueName);

                if (league == null)
                {
                    return ErrorResponse("League not found");
                }

                var users = new List<string>();

                if (string.IsNullOrEmpty(league.ClubName))
                {
                    users.AddRange(league.AcceptedUserNames);
                }
                else
                {
                    var clubRepository = (IClubRepository)services.GetService(typeof(IClubRepository));
                    var club = clubRepository.Load(league.ClubName);

                    if (club == null)
                    {
                        return ErrorResponse("Club not found");
                    }

                    users.AddRange(club.Members);
                }

                // Repositories we need to get score information
                var gameDetails = new GameDetails.GameDetails(services);
                var leaderboard = new GetLeaderboard.GetLeaderboard(services);

                var response = new LeagueScoresResponse();
                response.LeagueResults = new Dictionary<string, PlayerLeagueResponse>();

                foreach (var game in league.GamesToPlay)
                {
                    var allScores = false;
                    if (game.Key == "-1")
                    {
                        // No settings chosen so we are full game.
                        allScores = true;
                    }

                    var responseScores = new List<SimpleScore>();

                    if (allScores)
                    {
                        // We are not specifying a setting Id so we pick up the scores from the general leaderboard.
                        responseScores = GetSimpleScores(users, leaderboard, game.Key);
                    }
                    else
                    {
                        // We have settings for the game. So get the details from the detailed scores
                        responseScores = GetDetailedScores(users, gameDetails, game);
                    }

                    var orderedScores = responseScores.OrderBy(x => x.Score);
                    var position = 1;

                    foreach (var score in orderedScores)
                    {
                        if (!response.LeagueResults.ContainsKey(score.UserName))
                        {
                            // This is the first score for the user
                            response.LeagueResults.Add(score.UserName, new PlayerLeagueResponse
                            {
                                GamesPlayed = 0,
                                FirstPlaces = 0,
                                SecondPlaces = 0,
                                ThirdPlaces = 0,
                                TotalPoints = 0,
                            });
                        }

                        // Add to the games played
                        response.LeagueResults[score.UserName].GamesPlayed++;

                        // Add to the position stats
                        switch (position)
                        {
                            case 1:
                                response.LeagueResults[score.UserName].FirstPlaces++;
                                break;
                            case 2:
                                response.LeagueResults[score.UserName].SecondPlaces++;
                                break;
                            case 3:
                                response.LeagueResults[score.UserName].ThirdPlaces++;
                                break;
                            default:
                                break;
                        }

                        // Add to the total points
                        if (position <= league.ScoresForPlace.Count)
                        {
                            // Our position is in the array of points for a place in the top X
                            response.LeagueResults[score.UserName].TotalPoints += league.ScoresForPlace[position.ToString()];
                        }
                        else
                        {
                            // Our position is not one with a specific score so we pick up the
                            // points for playing
                            response.LeagueResults[score.UserName].TotalPoints += league.ScoreForPlaying;
                        }

                        position++;
                    }
                }

                return OkResponse(response);
            }
            catch (Exception e)
            {
                return ErrorResponse($"We were in our code but failed: {e.Message}");
            }
        }

        private static List<SimpleScore> GetSimpleScores(
            List<string> users,
            GetLeaderboard.GetLeaderboard leaderboard,
            string gameName)
        {
            var simpleScores = leaderboard.GetLeaderboardScores(gameName);
            var responseScores = new List<SimpleScore>();

            foreach (var score in simpleScores)
            {
                if (users.Contains(score.Username))
                {
                    responseScores.Add(new SimpleScore
                    {
                        UserName = score.Username,
                        Score = score.Score,
                        LevelName = "FULL GAME",
                    });
                }
            }

            return responseScores;
        }

        private static List<SimpleScore> GetDetailedScores(
        List<string> users,
        GameDetails.GameDetails gameDetails,
        KeyValuePair<string, string> game)
        {
            var responseScores = new List<SimpleScore>();
            var scores = gameDetails.GetAllScores(game.Key);

            foreach (var simpleScore in scores.SimpleScores)
            {
                // simpleScore.Key is the setting key that we are after
                if (simpleScore.Key == game.Value)
                {
                    foreach (var score in simpleScore.Value)
                    {
                        if (users.Contains(score.UserName) && score.LevelName == "FULL GAME")
                        {
                            responseScores.Add(score);
                        }
                    }
                }
            }

            return responseScores;
        }

        private APIGatewayProxyResponse OkResponse(object scores)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(scores),
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
