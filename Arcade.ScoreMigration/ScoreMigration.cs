using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Repositories;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.ScoreMigration
{
    public class ScoreMigration
    {
        private IServiceProvider services;

        public ScoreMigration()
            : this(DI.Container.Services())
        {
        }

        public ScoreMigration(IServiceProvider services)
        {
            this.services = services;
            ((IUserRepository)this.services.GetService(typeof(IUserRepository))).SetupTable();
            ((IObjectRepository)this.services.GetService(typeof(IObjectRepository))).SetupTable();
        }

        public APIGatewayProxyResponse ScoreMigrationHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var users = new List<string>
            {
                "DRUMZEY", "CHARLIEFAR", "FATOY", "GED", "GTBFILMS", "GUSFU", "HAPPYDUDE", "IAMJIMMI", "IMAJEN8TION", "MATTHEW_BRIDGE", "MICBASS", "MR20TO5",
                "RICHIERICHJTS", "RMB", "SHAUNHOLLEY", "SPECTRON", "STEREODELUXE", "SWISS_LIS", "TRONADS", "VIGILANTE", "WOTNOGRAVY", "IANCULLEN",
            };

            foreach (string username in users)
            {
                var userInfo = ((IUserRepository)services.GetService(typeof(IUserRepository))).Load(username);

                foreach (string gameKey in userInfo.Games.Keys)
                {
                    var scoreInformationForGame = ((IObjectRepository)services.GetService(typeof(IObjectRepository)))
                        .Load(gameKey);

                    if (scoreInformationForGame == null)
                    {
                        scoreInformationForGame = new ObjectInformation();
                        scoreInformationForGame.Key = gameKey;
                        scoreInformationForGame.DictionaryValue = new Dictionary<string, string>();
                        scoreInformationForGame.DictionaryValue.Add(username, userInfo.Games[gameKey]);
                    }
                    else
                    {
                        if (!scoreInformationForGame.DictionaryValue.ContainsKey(username))
                        {
                            scoreInformationForGame.DictionaryValue.Add(username, userInfo.Games[gameKey]);
                        }
                        else
                        {
                            scoreInformationForGame.DictionaryValue[username] = userInfo.Games[gameKey];
                        }
                    }

                    ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Save(scoreInformationForGame);
                }
            }

            return Response();
        }

        private APIGatewayProxyResponse Response()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "{ \"message\": \"Done\"}",
            };
        }
    }
}
