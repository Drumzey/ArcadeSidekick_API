using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
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
            ((IGameRepository)this.services.GetService(typeof(IGameRepository))).SetupTable();
        }

        public void AutoTweetLeaderboardsHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var category = GetCategory();
                var game = ((IGameRepository)services.GetService(typeof(IGameRepository))).Load(category);
                var rnd = new Random();
                int index = rnd.Next(0, game.Games.Count());
                var gameName = game.Games[index];
                var keyGameName = gameName.Replace(" ", "_").ToLower();

                var gameLeaderboard = ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Load(keyGameName);
                List<string> topFive = new List<string>();
                if (gameLeaderboard != null)
                {
                    topFive = GetTop5Scores(gameLeaderboard.DictionaryValue);
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

                game.Games.RemoveAt(index);

                if (game.TweetedGames == null)
                {
                    game.TweetedGames = new List<string>();
                }

                game.TweetedGames.Add(gameName);

                if (game.Games.Count() == 0)
                {
                    game.Games = game.TweetedGames;
                    game.TweetedGames = new List<string>();
                }

                ((IGameRepository)services.GetService(typeof(IGameRepository))).Save(game);
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

        private List<string> GetTop5Scores(Dictionary<string, string> dictionaryValue)
        {
            var numericVersions = dictionaryValue.ToDictionary(g => g.Key, g => Convert.ToDouble(g.Value));

            List<string> orderedGames = new List<string>();

            foreach (KeyValuePair<string, double> score in numericVersions.OrderByDescending(score => score.Value))
            {
                if (score.Value != 0)
                {
                    orderedGames.Add($"{score.Key}: {score.Value}");
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
