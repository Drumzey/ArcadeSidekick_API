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

namespace Arcade.AutoTweetCategoryGamer
{
    public class AutoTweetCategoryGamer
    {
        private IServiceProvider services;
        private IEnvironmentVariables environmentVariables;

        public AutoTweetCategoryGamer()
            : this(DI.Container.Services())
        {
        }

        public AutoTweetCategoryGamer(IServiceProvider services)
        {
            this.services = services;
            environmentVariables = (IEnvironmentVariables)this.services.GetService(typeof(IEnvironmentVariables));
            ((IObjectRepository)this.services.GetService(typeof(IObjectRepository))).SetupTable();
            ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).SetupTable();
        }

        public void AutoTweetCategoryGamerHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if(DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                // Dont do this on sunday
                // On sunday we will do the overall leaderboard
                return;
            }

            var scoreRepo = ((IObjectRepository)this.services.GetService(typeof(IObjectRepository))).AllScores();

            var nextGame = ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).Load("Category", "Games");

            //Extract the two categorys
            var firstGameCategory = nextGame.List1[0];
            var secondGameCategory = nextGame.List1[1];

            var categoryList = new List<string>
            {
                firstGameCategory,
                secondGameCategory
            };

            foreach(string category in categoryList)
            {
                if(nextGame.List2 == null)
                {
                    nextGame.List2 = new List<string>();
                }

                nextGame.List2.Add(category);
                nextGame.List1.Remove(category);

                var gamesCategory = ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).Load("Games", category + "_keys");
                Dictionary<string, int> bestPlayers = new Dictionary<string, int>();

                foreach (ObjectInformation gameInfo in scoreRepo)
                {
                    try
                    {
                        if (gamesCategory.List1.Contains(gameInfo.Key))
                        {
                            var numericVersions = gameInfo.DictionaryValue.ToDictionary(g => g.Key, g => Convert.ToDouble(g.Value));
                            var topPlayer = numericVersions.OrderByDescending(score => score.Value).First().Key;

                            if (!bestPlayers.ContainsKey(topPlayer))
                            {
                                bestPlayers.Add(topPlayer, 0);
                            }
                            bestPlayers[topPlayer]++;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error Processing {gameInfo.Key}");
                    }
                }

                var top5Players = bestPlayers.OrderByDescending(x => x.Value).Take(5).ToList();
                var categoryDescription = GetCategoryDescription(category);
                var message = GetTweetMessage(top5Players.Select(x => x.Key).ToList(), categoryDescription, "#arcade");
                Console.WriteLine(message);

                if (!string.IsNullOrEmpty(environmentVariables.TweetsOn))
                {
                    var service = new TwitterService(environmentVariables.ConsumerAPIKey, environmentVariables.ConsumerAPISecretKey);
                    service.AuthenticateWith(environmentVariables.AccessToken, environmentVariables.AccessTokenSecret);

                    if (message != null)
                    {
                        service.SendTweet(new SendTweetOptions
                        {
                            Status = message,
                        });
                    }
                }
            }

            ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).Save(nextGame);

            if (nextGame.List1.Count() == 0)
            {
                //We have done all the games in our category lists
                //Copy from list 2 back into list 1
                nextGame.List1 = new List<string>(nextGame.List2);
                //Clear out list 2
                nextGame.List2 = new List<string>();
            };

            ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).Save(nextGame);
        }

        private static string GetCategoryDescription(string firstGameCategory)
        {
            var categoryDescription = "";

            switch (firstGameCategory)
            {
                case "shooter":
                    categoryDescription = "Shmup";
                    break;
                case "sports":
                    categoryDescription = "Sports game";
                    break;
                case "runandgun":
                    categoryDescription = "Run'n'Gun";
                    break;
                case "puzzle":
                    categoryDescription = "Puzzle";
                    break;
                case "racing":
                    categoryDescription = "Racing";
                    break;
                case "platformer":
                    categoryDescription = "Platformer";
                    break;
                case "maze":
                    categoryDescription = "Maze";
                    break;
                case "guns":
                    categoryDescription = "Light gun";
                    break;
                case "hackandslash":
                    categoryDescription = "Hack'n'Slash";
                    break;
                case "beatemup":
                    categoryDescription = "Beat'em up";
                    break;
                case "fighting":
                    categoryDescription = "Fighting";
                    break;
                case "misc":
                    categoryDescription = "Misc";
                    break;
            }

            return categoryDescription + GetRandomDescription();
        }

        private static string GetRandomDescription()
        {
            List<string> descriptions = new List<string> {
                "aficionados",
                "connoisseurs",
                "experts",
                "authorities",
                "specialists",
                "cognoscentes",
                "fanatics",
                "savants",
                "enthusiast",
                "addicts",
                "buffs",
                "freaks",
                "nuts",
                "zealots",
                "fiends"
            };

            Random rnd = new Random();
            int option = rnd.Next(0, descriptions.Count - 1);

            return " " + descriptions[option];
        }

        private string GetTweetMessage(List<string> topFive, string gameType, string hashtag)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"The current #Top5 {gameType} (based on number of 1st place scores):");
            builder.AppendLine(string.Empty);

            foreach (string game in topFive)
            {
                builder.AppendLine(game);
            }

            builder.AppendLine(string.Empty);
            builder.Append($"Get challenging these players! {hashtag} #highscore #retrogames");
            return builder.ToString();
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
    }
}
