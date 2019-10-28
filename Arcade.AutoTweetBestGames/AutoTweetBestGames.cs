using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Misc;
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
            ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).SetupTable();
        }

        public void AutoTweetBestGamesHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var arcadeRatings = ((IMiscRepository)services.GetService(typeof(IMiscRepository)))
                    .Load("Ratings", "Arcade Games");
                var pinballRatings = ((IMiscRepository)services.GetService(typeof(IMiscRepository)))
                    .Load("Ratings", "Pinball");
                var averageOfAllVotes = ((IMiscRepository)services.GetService(typeof(IMiscRepository)))
                    .Load("Ratings", "Average");

                var topArcades = GetTop5WeightedAverage(arcadeRatings.Dictionary, averageOfAllVotes.Value);
                var topPinball = GetTop5WeightedAverage(pinballRatings.Dictionary, averageOfAllVotes.Value);

                var arcadeMessage = GetTweetMessage(topArcades, "arcade games", "#arcade");
                var pinballMessage = GetTweetMessage(topPinball, "pinball games", "#pinball");

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

                    if (pinballMessage != null)
                    {
                        service.SendTweet(new SendTweetOptions
                        {
                            Status = pinballMessage,
                        });
                    }
                }
                else
                {
                    Console.Write(arcadeMessage);
                    Console.Write(pinballMessage);
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private List<string> GetTop5WeightedAverage(Dictionary<string, string> dictionaryValue, string mean)
        {
            var numericVersions = dictionaryValue.ToDictionary(
                g => g.Key,
                g => WeightedAverageCalculator.CalculateAverage(
                    double.Parse(g.Value.Split(',')[0]),
                    double.Parse(g.Value.Split(',')[1]),
                    double.Parse(mean)));

            List<string> orderedGames = new List<string>();

            foreach (KeyValuePair<string, double> game in numericVersions.OrderByDescending(game => game.Value))
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

        private string GetTweetMessage(List<string> topFive, string gameType, string hashtag)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"The current #Top5 {gameType} as rated by sidekick users:");
            builder.AppendLine(string.Empty);

            foreach (string game in topFive)
            {
                builder.AppendLine(game);
            }

            builder.AppendLine(string.Empty);
            builder.Append($"If you dont see your favourite go vote for it in app! {hashtag} #highscore #retrogames");
            return builder.ToString();
        }
    }
}
