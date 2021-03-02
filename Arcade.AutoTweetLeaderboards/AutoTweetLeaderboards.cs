using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Misc;
using Arcade.Shared.Repositories;
using TweetSharp;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.AutoTweetLeaderboards
{
    public class AutoTweetLeaderboards
    {
        private IServiceProvider services;
        private IEnvironmentVariables environmentVariables;

        public AutoTweetLeaderboards()
            : this(DI.Container.Services())
        {
        }

        public AutoTweetLeaderboards(IServiceProvider services)
        {
            this.services = services;
            environmentVariables = (IEnvironmentVariables)this.services.GetService(typeof(IEnvironmentVariables));
            ((IObjectRepository)this.services.GetService(typeof(IObjectRepository))).SetupTable();
            ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).SetupTable();
        }

        public void AutoTweetLeaderboardsHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var category = GetCategory();
                var game = ((IMiscRepository)services.GetService(typeof(IMiscRepository)))
                    .Load("Games", category);
                var twitterHandles = ((IMiscRepository)services.GetService(typeof(IMiscRepository)))
                    .Load("Twitter", "Users");

                if (twitterHandles == null)
                {
                    twitterHandles = new Misc();
                    twitterHandles.Dictionary = new Dictionary<string, string>();
                }

                var rnd = new Random();
                int index = rnd.Next(0, game.List1.Count());
                var gameName = game.List1[index];
                var keyGameName = gameName.Replace(" ", "_").ToLower();

                var gameLeaderboard = ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Load(keyGameName);
                List<string> topFive = new List<string>();
                if (gameLeaderboard != null)
                {
                    topFive = GetTop5Scores(gameLeaderboard.DictionaryValue, twitterHandles.Dictionary);
                }

                var message = GetTweetMessage(topFive, gameName);

                if (message == null)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(environmentVariables.TweetsOn))
                {
                    var service = new TwitterService(environmentVariables.ConsumerAPIKey, environmentVariables.ConsumerAPISecretKey);
                    service.AuthenticateWith(environmentVariables.AccessToken, environmentVariables.AccessTokenSecret);

                    service.SendTweet(new SendTweetOptions
                    {
                        Status = message,
                    });
                }

                game.List1.RemoveAt(index);

                if (game.List2 == null)
                {
                    game.List2 = new List<string>();
                }

                game.List2.Add(gameName);

                if (game.List1.Count() == 0)
                {
                    game.List1 = game.List2;
                    game.List2 = new List<string>();
                }

                ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Save(game);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private string GetCategory()
        {
            var categories = environmentVariables.Categories.Split(',');

            var rnd = new Random();
            int choice = rnd.Next(0, categories.Length);

            return categories[choice];
        }

        private List<string> GetTop5Scores(Dictionary<string, string> dictionaryValue, Dictionary<string, string> twitterhandles)
        {
            var numericVersions = dictionaryValue.ToDictionary(g => g.Key, g => Convert.ToDouble(g.Value));

            List<string> orderedGames = new List<string>();

            foreach (KeyValuePair<string, double> score in numericVersions.OrderByDescending(score => score.Value))
            {
                if (score.Value != 0)
                {
                    if (twitterhandles.ContainsKey(score.Key))
                    {
                        orderedGames.Add($"{score.Key} @{twitterhandles[score.Key]}: {score.Value}");
                    }
                    else
                    {
                        orderedGames.Add($"{score.Key}: {score.Value}");
                    }
                }
            }

            return orderedGames.Take(5).ToList();
        }

        private string GetTweetMessage(List<string> topFive, string name)
        {
            var builder = new StringBuilder();

            if (topFive.Count == 0)
            {
                builder.AppendLine($"#Highscore time, at least it would be if we had any submissions for {name}!");
                builder.AppendLine($"This is your chance to be number one on the leaderboards, you might not get that chance again!");
                builder.Append("Submit your best and start the competition! #arcade #retrogames");
                return builder.ToString();
            }
            else if (topFive.Count < 5)
            {
                builder.AppendLine($"#Highscore time. Not many submissions for this one. The current #Top{topFive.Count} scores for {name} are:");
            }
            else
            {
                builder.AppendLine($"#Highscore time. The #Top5 scores for {name} are:");
            }

            builder.AppendLine(string.Empty);

            foreach (string game in topFive)
            {
                builder.AppendLine(game);
            }

            builder.AppendLine(string.Empty);
            var rnd = new Random();
            int choice = rnd.Next(0, 5);

            switch (choice)
            {
                case 0:
                    builder.Append("Why not have a go and see what place you can get? #arcade #retrogames");
                    break;
                case 1:
                    builder.Append("You can beat these surely? Have a go! #arcade #retrogames");
                    break;
                case 2:
                    builder.Append("These dont look too challenging do they? #arcade #retrogames");
                    break;
                case 3:
                    builder.Append("Easy to beat right? #arcade #retrogames");
                    break;
                case 4:
                    builder.Append("Submit your best and join our leaderboards! #arcade #retrogames");
                    break;
                default:
                    break;
            }

            return builder.ToString();
        }
    }
}
