using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using TweetSharp;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.AutoTweetTotals
{
    public class AutoTweetTotals
    {
        private IServiceProvider services;
        private IEnvironmentVariables environmentVariables;

        public AutoTweetTotals()
            : this(DI.Container.Services())
        {
        }

        public AutoTweetTotals(IServiceProvider services)
        {
            this.services = services;
            environmentVariables = (IEnvironmentVariables)this.services.GetService(typeof(IEnvironmentVariables));
            ((IUserRepository)this.services.GetService(typeof(IUserRepository))).SetupTable();
        }

        public void AutoTweetTotalsHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var conditions = new List<ScanCondition>();
            var results = ((IUserRepository)this.services.GetService(typeof(IUserRepository))).Scan(conditions);

            var totalScores = 0;
            var totalRatings = 0;

            foreach (UserInformation item in results)
            {
                if (item.NumberOfScoresUploaded != 0)
                {
                    totalScores += item.NumberOfScoresUploaded;
                }
                else
                {
                    var numericVersions = item.Games.ToDictionary(g => g.Key, g => Convert.ToDouble(g.Value));
                    var count = numericVersions.Where(x => x.Value > 0).Count();
                    totalScores += count;
                }

                if (item.NumberOfRatingsGiven != 0)
                {
                    totalRatings += item.NumberOfRatingsGiven;
                }
                else
                {
                    var count = item.Ratings.Where(x => x.Value > 0).Count();
                    totalRatings += count;
                }
            }

            var message1 = GetTweetMessageTotalScores(totalScores);
            var message2 = GetTweetMessageTotalRatings(totalRatings);

            if (!string.IsNullOrEmpty(environmentVariables.TweetsOn))
            {
                var service = new TwitterService(environmentVariables.ConsumerAPIKey, environmentVariables.ConsumerAPISecretKey);
                service.AuthenticateWith(environmentVariables.AccessToken, environmentVariables.AccessTokenSecret);

                if (message1 != null)
                {
                    service.SendTweet(new SendTweetOptions
                    {
                        Status = message1,
                    });
                }

                if (message2 != null)
                {
                    service.SendTweet(new SendTweetOptions
                    {
                        Status = message2,
                    });
                }
            }
            else
            {
                Console.Write(message1);
                Console.Write(message2);
            }
        }

        private string GetTweetMessageTotalScores(int totalScores)
        {
            var builder = new StringBuilder();

            builder.AppendLine("Its #Staturday, time for an update on our usage statistics.");
            builder.Append($"Through the app our users have now logged {totalScores} scores! ");

            Random rnd = new Random();
            int option = rnd.Next(1, 4);

            switch (option)
            {
                case 1:
                    builder.Append("Why not have a go at a new game and help us increase these numbers?");
                    break;
                case 2:
                    builder.Append("Go check your friends leaderboard and see if you can get to the top spot and help us push this number up.");
                    break;
                case 3:
                    builder.Append("Check you've uploaded your scores and help us make this number grow!");
                    break;
            }

            builder.Append(" #arcade #highscore #retrogames");
            return builder.ToString();
        }

        private string GetTweetMessageTotalRatings(int totalRatings)
        {
            var builder = new StringBuilder();

            builder.AppendLine("Its #Staturday, time for an update on our usage statistics.");
            builder.Append($"Through the app our users have now submitted {totalRatings} game ratings! ");

            Random rnd = new Random();
            int option = rnd.Next(1, 4);

            switch (option)
            {
                case 1:
                    builder.Append("The more ratings we have, the more accurate we can be! Dont forget to rate every game that you play.");
                    break;
                case 2:
                    builder.Append("Dont forget to rate your favourite games and use our top 50 to find new favourites.");
                    break;
                case 3:
                    builder.Append("Dont see your favourite up there? Dont forget to rate it and other games you play.");
                    break;
            }

            builder.Append(" #arcade #highscore #retrogames");
            return builder.ToString();
        }
    }
}
