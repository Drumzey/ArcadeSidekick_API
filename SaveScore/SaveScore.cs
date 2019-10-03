using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Messages;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.SaveScore
{
    public class SaveScore
    {
        private IServiceProvider services;

        public SaveScore()
            : this(DI.Container.Services())
        {
        }

        public SaveScore(IServiceProvider services)
        {
            this.services = services;
            ((IUserRepository)this.services.GetService(typeof(IUserRepository))).SetupTable();
            ((IObjectRepository)this.services.GetService(typeof(IObjectRepository))).SetupTable();
            ((IMessageRepository)this.services.GetService(typeof(IMessageRepository))).SetupTable();
        }

        public APIGatewayProxyResponse SaveScoreHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var userInfo = SaveUserInfo(request.Body, request.Headers["Authorization"]);
            SaveScoreInfo(request.Body);
            SaveRecentActivity(request.Body);

            return Response(userInfo);
        }

        private void SaveRecentActivity(string requestBody)
        {
            var data = JsonConvert.DeserializeObject<SaveUserInformationInput>(requestBody);

            var recentActivity = ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Load("recent");

            if (recentActivity == null)
            {
                recentActivity = new ObjectInformation();
                recentActivity.Key = "recent";
                recentActivity.ListValue = new System.Collections.Generic.List<string>();
            }

            foreach (string gameKey in data.Games.Keys)
            {
                try
                {
                    if (data.Games[gameKey] != "0" && data.Games[gameKey] != null)
                    {
                        var score = string.Format("{0:N0}", Convert.ToDouble(data.Games[gameKey]));

                        if (gameKey == "quick_and_crash" || gameKey == "neo_drift_out_new_technology")
                        {
                            TimeSpan t = TimeSpan.FromMilliseconds(double.Parse(data.Games[gameKey]));
                            score = string.Format(
                                "{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                t.Hours,
                                t.Minutes,
                                t.Seconds,
                                t.Milliseconds);
                        }

                        var message = GetMessage(gameKey, data.Username, score, DateTime.UtcNow.ToString("dd/MM/yyyy h:mm tt"));
                        recentActivity.ListValue.Add(message);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error uploadingrecent activity for game " + gameKey);
                    Console.WriteLine(e.Message);
                }
            }

            var newList = recentActivity.ListValue.Skip(Math.Max(0, recentActivity.ListValue.Count() - 100));
            recentActivity.ListValue = newList.ToList();

            ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Save(recentActivity);
        }

        private string GetGameName(string gameKey)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            var name = textInfo.ToTitleCase(gameKey.Replace("_", " "));

            if (name.Contains(" Xi ") || name.EndsWith(" Xi"))
            {
                name = name.Replace(" Xi ", " XI ");
                name = name.Replace(" Xi", " XI");
            }
            else if (name.Contains(" Viii ") || name.EndsWith(" Viii"))
            {
                name = name.Replace(" Viii ", " VIII ");
                name = name.Replace(" Viii", " VIII");
            }
            else if (name.Contains(" Vii ") || name.EndsWith(" Vii"))
            {
                name = name.Replace(" Vii ", " VII ");
                name = name.Replace(" Vii", " VII");
            }
            else if (name.Contains(" Vi ") || name.EndsWith(" Vi"))
            {
                name = name.Replace(" Vi ", " VI ");
                name = name.Replace(" Vi", " VI");
            }
            else if (name.Contains(" Iii ") || name.EndsWith(" Iii"))
            {
                name = name.Replace(" Iii ", " III ");
                name = name.Replace(" Iii", " III");
            }
            else if (name.Contains(" Ii ") || name.EndsWith(" Ii"))
            {
                name = name.Replace(" Ii ", " II ");
                name = name.Replace(" Ii", " II");
            }

            return name;
        }

        private string GetMessage(string gameKey, string username, string score, string date)
        {
            var name = GetGameName(gameKey);
            Console.Write($"{date}: {username} - {name} - {score}");
            return $"{date}: {username} - {name} - {score}";
        }

        private void SaveScoreInfo(string requestBody)
        {
            var data = JsonConvert.DeserializeObject<SaveUserInformationInput>(requestBody);

            foreach (string gameKey in data.Games.Keys)
            {
                var scoreInformationForGame = ((IObjectRepository)services.GetService(typeof(IObjectRepository)))
                    .Load(gameKey);

                if (scoreInformationForGame == null)
                {
                    scoreInformationForGame = new ObjectInformation();
                    scoreInformationForGame.Key = gameKey;
                    scoreInformationForGame.DictionaryValue = new System.Collections.Generic.Dictionary<string, string>();
                    scoreInformationForGame.DictionaryValue.Add(data.Username, data.Games[gameKey]);
                }
                else
                {
                    var oldTop3 = GetTopThree(scoreInformationForGame);

                    if (!scoreInformationForGame.DictionaryValue.ContainsKey(data.Username))
                    {
                        scoreInformationForGame.DictionaryValue.Add(data.Username, data.Games[gameKey]);
                    }
                    else
                    {
                        scoreInformationForGame.DictionaryValue[data.Username] = data.Games[gameKey];
                    }

                    var newTop3 = GetTopThree(scoreInformationForGame);

                    CreateMessage(oldTop3, newTop3, data.Username, gameKey);
                }

                ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Save(scoreInformationForGame);
            }
        }

        private List<string> GetTopThree(ObjectInformation currentGameScores)
        {
            var numericVersions = currentGameScores.DictionaryValue.ToDictionary(g => g.Key, g => Convert.ToDouble(g.Value));

            List<string> orderedGames = new List<string>();

            foreach (KeyValuePair<string, double> score in numericVersions.OrderByDescending(score => score.Value))
            {
                if (score.Value != 0)
                {
                    orderedGames.Add(score.Key);
                }
            }

            return orderedGames.Take(3).ToList();
        }

        private void CreateMessage(List<string> oldTop3, List<string> newTop3, string userName, string gameName)
        {
            // We are not in the top 3 so nothing has changed
            if (newTop3.IndexOf(userName) == -1)
            {
                return;
            }

            var indexOfNewPlayerInOldList = oldTop3.IndexOf(userName);
            var indexOfNewPlayerInNewList = newTop3.IndexOf(userName);

            if (indexOfNewPlayerInOldList == indexOfNewPlayerInNewList)
            {
                // Our position is unchanged
                return;
            }

            // We were not present in the old top 3 but are in the new top 3
            if (indexOfNewPlayerInOldList == -1 && indexOfNewPlayerInNewList != -1)
            {
                // We are now in first and the old list had players
                if (indexOfNewPlayerInNewList == 0 && oldTop3.Count > 0)
                {
                    // Create message to tell old first place that they are no longer in first
                    CreateFirstPlaceMessage(services, oldTop3[0], gameName, userName);
                }

                // Create message to tell person who was in third, that they have been knocked from the top 3
                if (oldTop3.Count >= 3)
                {
                    CreateNoLongerTop3PlaceMessage(services, oldTop3[2], gameName, userName);
                }
            }

            // We were in the old list and the new list in a new position
            if (indexOfNewPlayerInOldList != -1 && indexOfNewPlayerInNewList != -1)
            {
                if (indexOfNewPlayerInNewList == 0 && oldTop3.Count > 0)
                {
                    // Create message to tell old first place that they are no longer in first
                    CreateFirstPlaceMessage(services, oldTop3[0], gameName, userName);
                }
            }
        }

        private void CreateFirstPlaceMessage(IServiceProvider services, string to, string gameKey, string userName)
        {
            var name = GetGameName(gameKey);

            Arcade.Shared.Messages.CreateMessage.Create(
                       services,
                       to,
                       "Arcade Sidekick",
                       $"{to} you have been knocked from top spot on {name} by {userName}.",
                       Shared.Messages.MessageTypeEnum.ScoreBeaten,
                       new Dictionary<string, string>
                       {
                            { "Game", name },
                            { "GameKey", gameKey },
                       });
        }

        private void CreateNoLongerTop3PlaceMessage(IServiceProvider services, string to, string gameKey, string userName)
        {
            var name = GetGameName(gameKey);

            Arcade.Shared.Messages.CreateMessage.Create(
                       services,
                       to,
                       "Arcade Sidekick",
                       $"{to} you are no longer in the top 3 on {name}. {userName} has jumped above you.",
                       Shared.Messages.MessageTypeEnum.ScoreBeaten,
                       new Dictionary<string, string>
                       {
                            { "Game", name },
                            { "GameKey", gameKey },
                       });
        }

        private UserInformation SaveUserInfo(string requestBody, string token)
        {
            var data = JsonConvert.DeserializeObject<SaveUserInformationInput>(requestBody);

            JwtSecurityToken jwtToken;
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            try
            {
                jwtToken = handler.ReadJwtToken(token);
            }
            catch
            {
                throw new Exception("Invalid JWT");
            }

            if (jwtToken.Id.ToLower() != data.Username.ToLower())
            {
                throw new Exception("Attempting to Save data for other user");
            }

            var userInformation = ((IUserRepository)services.GetService(typeof(IUserRepository))).Load(data.Username);

            if (userInformation == null)
            {
                userInformation = new UserInformation
                {
                    Username = data.Username,
                    Games = data.Games,
                    Ratings = data.Ratings,
                    CreatedAt = DateTime.Now,
                };
            }
            else
            {
                foreach (string gameKey in data.Games.Keys)
                {
                    if (userInformation.Games.ContainsKey(gameKey))
                    {
                        userInformation.Games[gameKey] = data.Games[gameKey];
                    }
                    else
                    {
                        userInformation.Games.Add(gameKey, data.Games[gameKey]);
                    }

                    if (userInformation.Ratings.ContainsKey(gameKey))
                    {
                        userInformation.Ratings[gameKey] = data.Ratings[gameKey];
                    }
                    else
                    {
                        userInformation.Ratings.Add(gameKey, data.Ratings[gameKey]);
                    }
                }
            }

            userInformation.UpdatedAt = DateTime.Now;

            userInformation.NumberOfRatingsGiven = userInformation.Ratings.Where(x => x.Value != 0).Count();

            if (userInformation.NumberOfScoresUploaded == 0)
            {
                userInformation.NumberOfScoresUploaded = userInformation.Games.Count(x => x.Value != "0");
            }
            else
            {
                userInformation.NumberOfScoresUploaded += data.Games.Count(x => x.Value != "0");
            }

            userInformation.NumberOfGamesPlayed = userInformation.Games.Count();

            ((IUserRepository)services.GetService(typeof(IUserRepository))).Save(userInformation);

            return userInformation;
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
    }
}
