using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using TweetSharp;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.AutoTweetUserNumbers
{
    public class AutoTweetUserNumbers
    {
        private IServiceProvider services;
        private IEnvironmentVariables environmentVariables;

        public AutoTweetUserNumbers()
            : this(DI.Container.Services())
        {
        }

        public AutoTweetUserNumbers(IServiceProvider services)
        {
            this.services = services;
            environmentVariables = (IEnvironmentVariables)this.services.GetService(typeof(IEnvironmentVariables));
            ((IObjectRepository)this.services.GetService(typeof(IObjectRepository))).SetupTable();
        }

        public void AutoTweetUserNumbersHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var userRow = ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Load("users");
                var previousUsers = ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Load("previoususers");

                var users = userRow.ListValue.Count;
                var previous = 0;

                if (previousUsers != null)
                {
                    previous = Convert.ToInt32(previousUsers.ListValue[0]);
                }

                var message = GetTweetMessage(users, previous);

                if (message == null)
                {
                    return;
                }

                var service = new TwitterService(environmentVariables.ConsumerAPIKey, environmentVariables.ConsumerAPISecretKey);
                service.AuthenticateWith(environmentVariables.AccessToken, environmentVariables.AccessTokenSecret);

                service.SendTweet(new SendTweetOptions
                {
                    Status = message,
                });
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private string GetTweetMessage(int users, int previous)
        {
            var builder = new StringBuilder();

            builder.AppendLine("Its #Staturday, time for an update on our usage statistics.");
            builder.Append($"We now have {users} users");

            Random rnd = new Random();

            if (previous == 0)
            {
                builder.AppendLine(".");
            }
            else
            {
                if (users == previous)
                {
                    int sameOption = rnd.Next(1, 4);

                    switch (sameOption)
                    {
                        case 1:
                            builder.AppendLine($", which is the same as last week :-(");
                            break;
                        case 2:
                            builder.AppendLine($", which is an impressive increase of 0!");
                            break;
                        case 3:
                            builder.AppendLine($", no difference form last time. Poor.");
                            break;
                    }
                }
                else
                {
                    int sameOption = rnd.Next(1, 4);

                    switch (sameOption)
                    {
                        case 1:
                            builder.AppendLine($", an increase of {users - previous} from last week.");
                            break;
                        case 2:
                            builder.AppendLine($", thats {users - previous} more than last week.");
                            break;
                        case 3:
                            builder.AppendLine($", so {users - previous} more players have joined the game!");
                            break;
                    }
                }

                var previousUsers = new ObjectInformation
                {
                    Key = "previoususers",
                    ListValue = new List<string>
                    {
                        users.ToString(),
                    },
                };

                ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Save(previousUsers);
            }

            int option = rnd.Next(1, 4);

            switch (option)
            {
                case 1:
                    builder.Append("Lets increase those numbers, remember to share your scores and tell your friends about the sidekick!");
                    break;
                case 2:
                    builder.Append("We can do better! Dont forget to tell your friends about the sidekick and share your scores on social media.");
                    break;
                case 3:
                    builder.Append("These are rookie numbers, remember to share your scores and challenge your friends to increase our userbase!");
                    break;
            }

            builder.Append(" #arcade #highscore #retrogames");
            return builder.ToString();
        }
    }
}
