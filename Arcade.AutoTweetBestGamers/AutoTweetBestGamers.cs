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

namespace Arcade.AutoTweetBestGamers
{
    public class AutoTweetBestGamers
    {
        private IServiceProvider services;
        private IEnvironmentVariables environmentVariables;

        public AutoTweetBestGamers()
            : this(DI.Container.Services())
        {
        }

        public AutoTweetBestGamers(IServiceProvider services)
        {
            this.services = services;
            environmentVariables = (IEnvironmentVariables)this.services.GetService(typeof(IEnvironmentVariables));
            ((IObjectRepository)this.services.GetService(typeof(IObjectRepository))).SetupTable();
            ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).SetupTable();
        }

        public void AutoTweetBestGamersHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var scoreRepo = ((IObjectRepository)this.services.GetService(typeof(IObjectRepository))).AllScores();
            var pinballGames = ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).Load("Games","Pinball_Keys");

            var pinballKeys = pinballGames.List1;

            //We have a list of all scores
            //For each one, we want to order the scores in numeric order and take the top one
            //Then add that name to a dictionary
            Dictionary<string, int> bestPlayers = new Dictionary<string, int>();
            Dictionary<string, List<string>> bestPlayersGames = new Dictionary<string, List<string>>();

            Dictionary<string, int> bestPlayersPinball = new Dictionary<string, int>();
            Dictionary<string, List<string>> bestPlayersGamesPinball = new Dictionary<string, List<string>>();

            foreach (ObjectInformation gameInfo in scoreRepo)
            {
                //Console.WriteLine($"Processing Game: {gameInfo.Key}");

                try
                {
                    var numericVersions = gameInfo.DictionaryValue.ToDictionary(g => g.Key, g => Convert.ToDouble(g.Value));
                    var topPlayer = numericVersions.OrderByDescending(score => score.Value).First().Key;

                    if (WeAreAPinballGame(gameInfo.Key, pinballKeys))
                    {
                        if (!bestPlayersPinball.ContainsKey(topPlayer))
                        {
                            bestPlayersPinball.Add(topPlayer, 0);
                            bestPlayersGamesPinball.Add(topPlayer, new List<string>());
                        }
                        bestPlayersPinball[topPlayer]++;
                        bestPlayersGamesPinball[topPlayer].Add(gameInfo.Key);
                    }
                    else
                    {
                        if (!bestPlayers.ContainsKey(topPlayer))
                        {
                            bestPlayers.Add(topPlayer, 0);
                            bestPlayersGames.Add(topPlayer, new List<string>());
                        }
                        bestPlayers[topPlayer]++;
                        bestPlayersGames[topPlayer].Add(gameInfo.Key);
                    }
                    
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Error Processing {gameInfo.Key}");
                }
            }

            var top5Players = bestPlayers.OrderByDescending(x => x.Value).Take(5).ToList();
            var top5PlayersPinball = bestPlayersPinball.OrderByDescending(x => x.Value).Take(5).ToList();

            Console.WriteLine("Top 1: " + top5Players[0].Key.ToString() + " " + top5Players[0].Value.ToString());
            Console.WriteLine("Top 2: " + top5Players[1].Key.ToString() + " " + top5Players[1].Value.ToString());
            Console.WriteLine("Top 3: " + top5Players[2].Key.ToString() + " " + top5Players[2].Value.ToString());
            Console.WriteLine("Top 4: " + top5Players[3].Key.ToString() + " " + top5Players[3].Value.ToString());
            Console.WriteLine("Top 5: " + top5Players[4].Key.ToString() + " " + top5Players[4].Value.ToString());

            Console.WriteLine("Top 1: " + top5PlayersPinball[0].Key.ToString() + " " + top5PlayersPinball[0].Value.ToString());
            Console.WriteLine("Top 2: " + top5PlayersPinball[1].Key.ToString() + " " + top5PlayersPinball[1].Value.ToString());
            Console.WriteLine("Top 3: " + top5PlayersPinball[2].Key.ToString() + " " + top5PlayersPinball[2].Value.ToString());
            Console.WriteLine("Top 4: " + top5PlayersPinball[3].Key.ToString() + " " + top5PlayersPinball[3].Value.ToString());
            Console.WriteLine("Top 5: " + top5PlayersPinball[4].Key.ToString() + " " + top5PlayersPinball[4].Value.ToString());

            var arcadeMessage = GetTweetMessage(top5Players.Select(x => x.Key).ToList(),"arcade","#arcade");
            var pinballMessage = GetTweetMessage(top5PlayersPinball.Select(x => x.Key).ToList(), "pinball", "#pinball");

            Console.WriteLine(arcadeMessage);
            Console.WriteLine(pinballMessage);

            if (!string.IsNullOrEmpty(environmentVariables.TweetsOn))
            {
                var service = new TwitterService(environmentVariables.ConsumerAPIKey, environmentVariables.ConsumerAPISecretKey);
                service.AuthenticateWith(environmentVariables.AccessToken, environmentVariables.AccessTokenSecret);

                if (arcadeMessage != null)
                {
                    service.SendTweet(new SendTweetOptions
                    {
                        Status = arcadeMessage,
                    });
                }
            }

            if (!string.IsNullOrEmpty(environmentVariables.TweetsOn))
            {
                var service = new TwitterService(environmentVariables.ConsumerAPIKey, environmentVariables.ConsumerAPISecretKey);
                service.AuthenticateWith(environmentVariables.AccessToken, environmentVariables.AccessTokenSecret);

                if (pinballMessage != null)
                {
                    service.SendTweet(new SendTweetOptions
                    {
                        Status = pinballMessage,
                    });
                }
            }
        }

        private bool WeAreAPinballGame(string key, List<string> pinballGames) => pinballGames.Contains(key);

        private string GetTweetMessage(List<string> topFive, string gameType, string hashtag)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"The current #Top5 {gameType} players (based on number of 1st place scores):");
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
