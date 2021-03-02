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
using Arcade.Shared.Misc;
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
            ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).SetupTable();
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

            var recentActivity = ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Load("Activity", "All");

            if (recentActivity == null)
            {
                recentActivity = new Misc();
                recentActivity.Key = "Activity";
                recentActivity.SortKey = "All";
                recentActivity.List1 = new System.Collections.Generic.List<string>();
            }

            foreach (string gameKey in data.Games.Keys)
            {
                try
                {
                    if (data.Games[gameKey] != "0" && !string.IsNullOrWhiteSpace(data.Games[gameKey]))
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
                        recentActivity.List1.Add(message);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error uploadingrecent activity for game " + gameKey);
                    Console.WriteLine(e.Message);
                }
            }

            var newList = recentActivity.List1.Skip(Math.Max(0, recentActivity.List1.Count() - 100));
            recentActivity.List1 = newList.ToList();

            ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Save(recentActivity);
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
                try
                {
                    if (string.IsNullOrWhiteSpace(data.Games[gameKey]))
                    {
                        // We are null or empty for some reason
                        // do not do anything with this score as it coudl be being written by mistake
                        continue;
                    }

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
                        List<string> oldTop3 = new List<string>();
                        List<string> oldTop5 = new List<string>();
                        List<string> oldTop10 = new List<string>();
                        List<string> oldTop25 = new List<string>();
                        List<string> oldTop50 = new List<string>();
                        List<string> oldTop100 = new List<string>();
                        List<string> allOldScores = new List<string>();

                        try
                        {
                            oldTop3 = GetTopX(3, scoreInformationForGame);
                            oldTop5 = GetTopX(5, scoreInformationForGame);
                            oldTop10 = GetTopX(10, scoreInformationForGame);
                            oldTop25 = GetTopX(25, scoreInformationForGame);
                            oldTop50 = GetTopX(50, scoreInformationForGame);
                            oldTop100 = GetTopX(100, scoreInformationForGame);
                            allOldScores = GetAll(scoreInformationForGame);
                        }
                        catch (Exception e)
                        {
                            Console.Write("Couldnt get old top X");
                        }

                        if (!scoreInformationForGame.DictionaryValue.ContainsKey(data.Username))
                        {
                            scoreInformationForGame.DictionaryValue.Add(data.Username, data.Games[gameKey]);
                        }
                        else
                        {
                            scoreInformationForGame.DictionaryValue[data.Username] = data.Games[gameKey];
                        }

                        try
                        {
                            var newTop3 = GetTopX(3, scoreInformationForGame);
                            var newTop5 = GetTopX(5, scoreInformationForGame);
                            var newTop10 = GetTopX(10, scoreInformationForGame);
                            var newTop25 = GetTopX(25, scoreInformationForGame);
                            var newTop50 = GetTopX(50, scoreInformationForGame);
                            var newTop100 = GetTopX(100, scoreInformationForGame);
                            var newAllScores = GetAll(scoreInformationForGame);

                            Console.Write("Writing first place message");
                            CreateFirstPlaceMessage(oldTop3, newTop3, data.Username, gameKey);
                            Console.Write("Sending 3rd place message");
                            CreateMessage(oldTop3, newTop3, data.Username, gameKey, 3);
                            Console.Write("Sending 5th place message");
                            CreateMessage(oldTop5, newTop5, data.Username, gameKey, 5);
                            Console.Write("Sending 10th place message");
                            CreateMessage(oldTop10, newTop10, data.Username, gameKey, 10);
                            Console.Write("Sending 25th place message");
                            CreateMessage(oldTop25, newTop25, data.Username, gameKey, 25);
                            Console.Write("Sending 50th place message");
                            CreateMessage(oldTop50, newTop50, data.Username, gameKey, 50);
                            Console.Write("Sending 100th place messages");
                            CreateMessage(oldTop100, newTop100, data.Username, gameKey, 100);

                            Console.Write("Sending friend messages");
                            DoMessagesForFriends(
                                allOldScores,
                                newAllScores,
                                data.Username,
                                GetGameName(gameKey),
                                gameKey);
                        }
                        catch (Exception e)
                        {
                            Console.Write("Couldnt send messages");
                        }
                    }

                    ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Save(scoreInformationForGame);
                }
                catch (Exception e)
                {
                    throw new Exception("Cannot create recent activity");
                }
            }
        }

        private void DoMessagesForFriends(
            List<string> allOldScores,
            List<string> newAllScores,
            string userName,
            string gameName,
            string gameKey)
        {
            var playerOldPosition = allOldScores.IndexOf(userName);
            var playerNewPosition = newAllScores.IndexOf(userName);
            var playersToPotentiallyNotify = new List<string>();

            if (playerOldPosition == -1)
            {
                // The player was not on the scoreboard, so any player in the positions behind the player
                // who is following them now needs to be notified
                playersToPotentiallyNotify = newAllScores.Skip(playerNewPosition + 1)
                    .Take(newAllScores.Count() - playerNewPosition + 1).ToList();
            }
            else
            {
                Console.Write("Getting players to notify");
                playersToPotentiallyNotify = newAllScores.Skip(playerNewPosition + 1)
                    .Take(playerOldPosition - playerNewPosition).ToList();
                Console.Write(playersToPotentiallyNotify);
            }

            // Get the players who follow the current player
            var peopleWhoFollowX = ((IMiscRepository)services
                .GetService(typeof(IMiscRepository))).Load("Followers", userName);
            Console.Write(peopleWhoFollowX);

            if (peopleWhoFollowX != null)
            {
                // Take the intersection of the two lists
                var playersToNotify = peopleWhoFollowX.List1.Intersect(playersToPotentiallyNotify);
                Console.Write(playersToNotify);

                // The result is the people who need to get a message
                foreach (string name in playersToNotify)
                {
                    var expletive = GetExpletive();
                    Console.Write(expletive);
                    Console.Write($"Sending message to {name}");
                    Arcade.Shared.Messages.CreateMessage.Create(
                       services,
                       name,
                       "Arcade Sidekick",
                       $"{expletive} {userName} has beaten your top score on {gameName}. Don't let them get away with it!",
                       Shared.Messages.MessageTypeEnum.ScoreBeaten,
                       new Dictionary<string, string>
                       {
                            { "Game", gameName },
                            { "GameKey", gameKey },
                       });
                }
            }
        }

        private string GetExpletive()
        {
            Random rnd = new Random();
            int result = rnd.Next(1, 11);

            switch (result)
            {
                case 1: return "Uh oh.";
                case 2: return "What a nuisance.";
                case 3: return "There is something concerning we need to discuss.";
                case 4: return "Sorry to be the bearer of some unfortunate news.";
                case 5: return "I have some disappointing news for you.";
                case 6: return "I have some distressing news.";
                case 7: return "@!#?@!";
                case 8: return "I am the bearer of bad news.";
                case 9: return "There's no easy way to say this.";
                case 10: return "Bad news!";
            }

            return "Oh poo.";
        }

        private List<string> GetAll(ObjectInformation currentGameScores)
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

            return orderedGames.ToList();
        }

        private List<string> GetTopX(int x, ObjectInformation currentGameScores)
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

            return orderedGames.Take(x).ToList();
        }

        private void CreateFirstPlaceMessage(List<string> oldTopX, List<string> newTopX, string userName, string gameName)
        {
            // We are not in the top 3 so nothing has changed
            if (newTopX.IndexOf(userName) == -1)
            {
                return;
            }

            var indexOfNewPlayerInOldList = oldTopX.IndexOf(userName);
            var indexOfNewPlayerInNewList = newTopX.IndexOf(userName);

            Console.WriteLine("Old place: " + indexOfNewPlayerInOldList);
            Console.WriteLine("New place: " + indexOfNewPlayerInNewList);

            if (indexOfNewPlayerInOldList == indexOfNewPlayerInNewList)
            {
                // Our position is unchanged
                Console.WriteLine("Position unchanged");
                return;
            }

            // We were not present in the old top X but are in the new top X
            if (indexOfNewPlayerInOldList == -1 && indexOfNewPlayerInNewList != -1)
            {
                // We are now in first and the old list had players
                if (indexOfNewPlayerInNewList == 0 && oldTopX.Count > 0)
                {
                    // Create message to tell old first place that they are no longer in first
                    Console.WriteLine("New first place!");
                    Console.WriteLine("Person to notify: " + oldTopX[0]);
                    CreateFirstPlaceMessage(services, oldTopX[0], gameName, userName);
                }
            }

            // We were in the old list and the new list in a new position
            if (indexOfNewPlayerInOldList != -1 && indexOfNewPlayerInNewList != -1)
            {
                Console.WriteLine("We were in the top three and still are");
                if (indexOfNewPlayerInNewList == 0 && indexOfNewPlayerInOldList != 0 && oldTopX.Count > 0)
                {
                    Console.WriteLine("We were not first but now we are");
                    Console.WriteLine("Old first place: " + oldTopX[0]);
                    // Create message to tell old first place that they are no longer in first
                    CreateFirstPlaceMessage(services, oldTopX[0], gameName, userName);
                    return;
                }
            }
        }

        private void CreateMessage(List<string> oldTopX, List<string> newTopX, string userName, string gameName, int number)
        {
            Console.WriteLine("U: " + userName);
            Console.WriteLine("G: " + gameName);
            Console.WriteLine("Old: " + string.Join(",", oldTopX));
            Console.WriteLine("New: " + string.Join(",", newTopX));

            // We are not in the top 3 so nothing has changed
            if (newTopX.IndexOf(userName) == -1)
            {
                return;
            }

            var indexOfNewPlayerInOldList = oldTopX.IndexOf(userName);
            var indexOfNewPlayerInNewList = newTopX.IndexOf(userName);

            Console.WriteLine("Old place: " + indexOfNewPlayerInOldList);
            Console.WriteLine("New place: " + indexOfNewPlayerInNewList);

            if (indexOfNewPlayerInOldList == indexOfNewPlayerInNewList)
            {
                // Our position is unchanged
                Console.WriteLine("Position unchanged");
                return;
            }

            // We were not present in the old top X but are in the new top X
            if (indexOfNewPlayerInOldList == -1 && indexOfNewPlayerInNewList != -1)
            {
                // Create message to tell person who was in X, that they have been knocked from the top X
                if (oldTopX.Count >= number)
                {
                    Console.WriteLine("X place knocked down");
                    Console.WriteLine("Person to notify: " + oldTopX[number - 1]);
                    CreateNoLongerTopXPlaceMessage(services, oldTopX[number - 1], gameName, userName, number);
                    return;
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

        private void CreateNoLongerTopXPlaceMessage(IServiceProvider services, string to, string gameKey, string userName, int number)
        {
            var name = GetGameName(gameKey);

            Arcade.Shared.Messages.CreateMessage.Create(
                       services,
                       to,
                       "Arcade Sidekick",
                       $"{to} you are no longer in the top {number} on {name}. {userName} has jumped above you.",
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
                    if (string.IsNullOrWhiteSpace(data.Games[gameKey]))
                    {
                        // We are null or empty for some reason
                        // do not do anything with this score as it coudl be being written by mistake
                        continue;
                    }
                    else
                    {
                        if (userInformation.Games.ContainsKey(gameKey))
                        {
                            userInformation.Games[gameKey] = data.Games[gameKey];
                        }
                        else
                        {
                            userInformation.Games.Add(gameKey, data.Games[gameKey]);
                        }
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
