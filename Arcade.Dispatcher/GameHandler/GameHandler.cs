using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.GetLeaderboard;
using Arcade.GetRating;
using Arcade.GetScore;
using Arcade.SaveRating;
using Arcade.SaveScore;
using Arcade.TopFifty;
using System;
using System.Net;

namespace Arcade.Dispatcher.GameHandler
{
    public static class GameHandler
    {
        public static APIGatewayProxyResponse HandleRequest(
            APIGatewayProxyRequest request,
            ILambdaContext context,
            IServiceProvider services)
        {
            //TODO: These call an existing dispatcher that has different validation on the path parameters.....
            //Need to update the paths in gamedetails to match 
            switch (request.Resource)
            {
                case "/app/games/allgames":
                    return ErrorResponse("AllGames not implemented.");

                case "/app/games/request":
                    return ErrorResponse("Request game not implemented.");

                case "/app/games/all":
                case "/app/games/availableat": //This game is available at
                case "/app/games/detailedscore":
                case "/app/games/knownlevels":
                case "/app/games/knownsettings":
                case "/app/games/ratingsweighted":
                    var levels = new GameDetails.GameDetails(services);
                    return levels.GameDetailsHandler(request, context);

                //Get and set the simple scores for a game
                case "/app/games/simplescore":
                    if (request.HttpMethod == "GET")
                    {
                        var getScore = new GetScore.GetScore(services);
                        return getScore.GetScoreHandler(request, context);
                    }
                    else if (request.HttpMethod == "POST")
                    {
                        var saveScore = new SaveScore.SaveScore(services);
                        return saveScore.SaveScoreHandler(request, context);
                    }
                    return ErrorResponse("Unknown method on ratings.");

                //Get and set the simple ratings for a game
                case "/app/games/ratings":
                    if (request.HttpMethod == "GET")
                    {
                        var getRatings = new GetRating.GetRating(services);
                        return getRatings.GetRatingHandler(request, context);
                    }
                    else if (request.HttpMethod == "POST")
                    {
                        var saveRatings = new SaveRating.SaveRating(services);
                        return saveRatings.SaveRatingHandler(request, context);
                    }
                    return ErrorResponse("Unknown method on ratings.");

                //Get the top 50 games for arcade and pinball
                case "/app/games/top50":
                    var top50 = new TopFifty.TopFifty(services);
                    return top50.TopFiftyHandler(request, context);

                //Get the simple leader board for a game
                case "/app/games/leaderboard":
                    var leaderboard = new GetLeaderboard.GetLeaderboard(services);
                    return leaderboard.GetLeaderboardHandler(request, context);

                default:
                    return ErrorResponse("Unknown Game endpoint.");
            }
        }

        private static APIGatewayProxyResponse ErrorResponse(string errorMessage)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"Error. " + errorMessage + "\"}",
            };
        }
    }
}
