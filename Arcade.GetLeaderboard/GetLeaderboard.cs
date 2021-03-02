using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Arcade.Shared.Shared;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.GetLeaderboard
{
    public class GetLeaderboard
    {
        private IServiceProvider services;

        public GetLeaderboard()
            : this(DI.Container.Services())
        {
        }

        public GetLeaderboard(IServiceProvider services)
        {
            this.services = services;
            ((IObjectRepository)this.services.GetService(typeof(IObjectRepository))).SetupTable();
            ((IClubRepository)this.services.GetService(typeof(IClubRepository))).SetupTable();
        }

        public List<SimpleScore> GetLeaderboardScores(string gameName)
        {
            var gamescore = ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Load(gameName);
            if (gamescore == null)
            {
                return new List<SimpleScore>();
            }

            return gamescore.DictionaryValue.Select(x => new SimpleScore
            {
                Username = x.Key,
                Score = x.Value,
            }).ToList();
        }

        public APIGatewayProxyResponse GetLeaderboardHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string gamename;
            string clubname;
            request.QueryStringParameters.TryGetValue("gamename", out gamename);
            request.QueryStringParameters.TryGetValue("clubname", out clubname);

            if (string.IsNullOrEmpty(gamename))
            {
                return ErrorResponse("No game name provided");
            }

            try
            {
                var gamescore = ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Load(gamename);
                if (gamescore == null)
                {
                    var emptyResponse = new ObjectInformation();
                    emptyResponse.DictionaryValue = new Dictionary<string, string>();
                    return OkResponse(emptyResponse.DictionaryValue);
                }

                if (string.IsNullOrEmpty(clubname))
                {
                    return OkResponse(gamescore.DictionaryValue);
                }
                else
                {
                    return FilterByClubName(gamescore.DictionaryValue, clubname);
                }
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        private APIGatewayProxyResponse FilterByClubName(Dictionary<string, string> scores, string clubName)
        {
            var filteredScores = new Dictionary<string, string>();
            var clubRepository = (IClubRepository)services.GetService(typeof(IClubRepository));

            // Fix to allow for & in club name
            if (clubName.StartsWith("Neon Knights Arcade"))
            {
                clubName = "Neon Knights Arcade & Cafe";
            }

            var club = clubRepository.Load(clubName);

            if (club == null)
            {
                return ErrorResponse($"{clubName}. Club not found");
            }

            foreach (string member in club.Members)
            {
                if (scores.ContainsKey(member))
                {
                    filteredScores.Add(member, scores[member]);
                }
            }

            return OkResponse(filteredScores);
        }

        private APIGatewayProxyResponse OkResponse(Dictionary<string, string> scores)
        {
            //var headers = new Dictionary<string, string>
            //{
            //    { "Access-Control-Allow-Origin", "*" },
            //};

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(scores),
                //Headers = headers,
            };
        }

        private APIGatewayProxyResponse ErrorResponse(string error)
        {
            var headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
            };

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = "{ \"message\": \"Error. " + error + "\"}",
                Headers = headers,
            };
        }
    }
}
