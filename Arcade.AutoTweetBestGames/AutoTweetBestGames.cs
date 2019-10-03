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

namespace Arcade.AutoTweetBestGames
{
    public class AutoTweetBestGames
    {
        private IServiceProvider services;
        private IEnvironmentVariables environmentVariables;

        public AutoTweetBestGames()
            : this(DI.Container.Services())
        {
        }

        public AutoTweetBestGames(IServiceProvider services)
        {
            this.services = services;
            environmentVariables = (IEnvironmentVariables)this.services.GetService(typeof(IEnvironmentVariables));
            ((IObjectRepository)this.services.GetService(typeof(IObjectRepository))).SetupTable();
        }

        public void AutoTweetBestGamesHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var ratings = ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Load("ratings");
                var topFive = GetTop5Ratings(ratings.DictionaryValue);
                var message = GetTweetMessage(topFive);

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
                else
                {
                    Console.Write(message);
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private List<string> GetTop5Ratings(Dictionary<string, string> dictionaryValue)
        {
            var numbericVersions = dictionaryValue.ToDictionary(g => g.Key, g => Convert.ToDouble(g.Value));

            List<string> orderedGames = new List<string>();

            foreach (KeyValuePair<string, double> game in numbericVersions.OrderByDescending(game => game.Value))
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                var name = textInfo.ToTitleCase(game.Key.Replace("_", " "));

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

                orderedGames.Add($"{name}: {Math.Round(game.Value, 2)}");
            }

            return orderedGames.Take(5).ToList();
        }

        private string GetTweetMessage(List<string> topFive)
        {
            var builder = new StringBuilder();

            builder.AppendLine("The current #Top5 games as rated by sidekick users:");
            builder.AppendLine(string.Empty);

            foreach (string game in topFive)
            {
                builder.AppendLine(game);
            }

            builder.AppendLine(string.Empty);
            builder.Append("If you dont see your favourite go vote for it in app! #arcade #highscore #retrogames");
            return builder.ToString();
        }
    }
}
