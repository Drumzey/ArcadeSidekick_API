using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.GameDetails;
using Arcade.Shared;
using Arcade.Shared.Misc;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.RestoreUser
{
    public class RestoreUser
    {
        private IServiceProvider services;

        public RestoreUser()
            : this(DI.Container.Services())
        {
        }

        public RestoreUser(IServiceProvider services)
        {
            this.services = services;
            ((IUserRepository)this.services.GetService(typeof(IUserRepository))).SetupTable();
            ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).SetupTable();
            ((IGameDetailsRepository)this.services.GetService(typeof(IGameDetailsRepository))).SetupTable();
        }

        public APIGatewayProxyResponse RestoreUserHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string username;
            request.QueryStringParameters.TryGetValue("username", out username);

            if (string.IsNullOrEmpty(username))
            {
                return ErrorResponse();
            }

            var userInfo = GetUserInfo(username);
            return Response(userInfo);
        }

        private UserInformation GetUserInfo(string username)
        {
            var userRepository = (IUserRepository)services.GetService(typeof(IUserRepository));
            var userInformation = userRepository.Load(username);

            userInformation.NumberOfGamesPlayed = userInformation.Games.Count();
            userInformation.NumberOfRatingsGiven = userInformation.Ratings.Where(x => x.Value != 0).Count();

            if (userInformation.NumberOfScoresUploaded == 0)
            {
                userInformation.NumberOfScoresUploaded = userInformation.Games.Where(x => x.Value != "0").Count();
            }

            userRepository.Save(userInformation);

            //Get friends....
            var miscRepository = (IMiscRepository)services.GetService(typeof(IMiscRepository));
            //We need to get all the names of users who have DRUMZEY as a follower
            var usersWithFollowers = miscRepository.QueryByFollowerName("Followers");

            userInformation.Friends = new List<string>();

            foreach (Misc misc in usersWithFollowers)
            {
                if (misc.List1.Contains(username))
                {
                    userInformation.Friends.Add(misc.SortKey);
                }
            }

            var detailedScores = GetDetailedScores(username);

            userInformation.DetailedScores = detailedScores;
            userInformation.DetailedSettings = GetDetailedSettings(detailedScores.Select(x => x.Key).ToList());

            return userInformation;
        }

        private Dictionary<string, List<ScoreDetails>> GetDetailedScores(string userName)
        {
            var returnDictionary = new Dictionary<string, List<ScoreDetails>>();
            //Get detailed scores
            var gameDetailsRepository = (IGameDetailsRepository)services.GetService(typeof(IGameDetailsRepository));
            //We need to get all the entries that have drumzey as the sort key
            var gamesForUser = gameDetailsRepository.AllGamesForUser(userName);

            foreach(var record in gamesForUser)
            {
                returnDictionary.Add(record.Game, new List<ScoreDetails>());
                returnDictionary[record.Game] = record.Scores;
            }

            return returnDictionary;
        }

        private Dictionary<string, List<Setting>> GetDetailedSettings(List<string> gameNames)
        {
            var returnDictionary = new Dictionary<string, List<Setting>>();
            var gameDetailsRepository = (IGameDetailsRepository)services.GetService(typeof(IGameDetailsRepository));

            foreach (string gameName in gameNames)
            {
                //We need to get all the entries that have drumzey as the sort key
                var settings = gameDetailsRepository.Load(gameName, "Settings");
                if (settings != null)
                {
                    returnDictionary.Add(gameName, new List<Setting>());
                    returnDictionary[gameName] = settings.Settings;
                }
            }

            return returnDictionary;
        }

        private APIGatewayProxyResponse Response(UserInformation userInfo)
        {
            userInfo.Secret = "***";

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(userInfo),
            };
        }

        private APIGatewayProxyResponse ErrorResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"Error no username supplied\" }",
            };
        }
    }
}
