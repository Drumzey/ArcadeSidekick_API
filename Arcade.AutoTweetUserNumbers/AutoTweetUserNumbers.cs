using System;
using System.IO;
using System.Net;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Misc;
using OAuth;
using static System.Net.WebRequestMethods;

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
            ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).SetupTable();
        }

        public void AutoTweetUserNumbersHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var userRow = ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Load("Activity", "Users");
                var previousUsers = ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Load("Activity", "PreviousUsers");

                var users = userRow.List1.Count;
                var previous = 0;

                if (previousUsers != null)
                {
                    previous = Convert.ToInt32(previousUsers.Value);
                }

                var message = GetTweetMessage(users, previous);

                if (message == null)
                {
                    return;
                }

                this.Tweet(message);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private string GetTweetMessage(int users, int previous)
        {
            var builder = new StringBuilder();

            Random rnd = new Random();

            if (users == previous)
            {
                int sameOption = rnd.Next(1, 4);

                switch (sameOption)
                {
                    case 1:
                        builder.Append($"Its #Staturday, time for an update on our usage statistics. We now have {users} users, which is the same as last week :-(");
                        break;
                    case 2:
                        builder.Append($"Its #Staturday, time for an update on our usage statistics. We now have {users} users, which is an impressive increase of 0!");
                        break;
                    case 3:
                        builder.Append($"Its #Staturday, time for an update on our usage statistics. We now have {users} users, no difference form last time. Poor.");
                        break;
                }
            }
            else
            {
                int sameOption = rnd.Next(1, 4);

                switch (sameOption)
                {
                    case 1:
                        builder.Append($"Its #Staturday, time for an update on our usage statistics. We now have {users} users, an increase of {users - previous} from last week");
                        break;
                    case 2:
                        builder.Append($"Its #Staturday, time for an update on our usage statistics. We now have {users} users, thats {users - previous} more than last week");
                        break;
                    case 3:
                        builder.Append($"Its #Staturday, time for an update on our usage statistics. We now have {users} users, so {users - previous} more players have joined the game!");
                        break;
                }
            }

            var previousUsers = new Misc
            {
                Key = "Activity",
                SortKey = "PreviousUsers",
                Value = users.ToString(),
            };

            ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Save(previousUsers);

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

        public void Tweet(string message)
        {
            var _oAuthConsumerKey = environmentVariables.ConsumerAPIKey;
            var _oAuthConsumerSecret = environmentVariables.ConsumerAPISecretKey;
            var _accessToken = environmentVariables.AccessToken;
            var _accessTokenSecret = environmentVariables.AccessTokenSecret;

            Console.WriteLine(_oAuthConsumerKey);
            Console.WriteLine(_oAuthConsumerSecret);
            Console.WriteLine(_accessToken);
            Console.WriteLine(_accessTokenSecret);

            OAuthRequest client = OAuthRequest.ForProtectedResource("POST", _oAuthConsumerKey, _oAuthConsumerSecret, _accessToken, _accessTokenSecret);
            client.RequestUrl = "https://api.twitter.com/2/tweets";
            string auth = client.GetAuthorizationHeader();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(client.RequestUrl);
            request.Method = "POST";
            request.Headers.Add("Content-Type", "application/json");
            request.Headers.Add("Authorization", auth);

            var jsonMessage = "{\"text\": \"" + message + "\"}";
            Console.WriteLine(jsonMessage);

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(jsonMessage);
            }

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            Console.WriteLine(responseString);
        }
    }
}
