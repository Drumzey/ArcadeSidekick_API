using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.Challenge_GET
{
    public class Challenge_GET
    {
        private IServiceProvider services;

        public Challenge_GET()
            : this(DI.Container.Services())
        {
        }

        public Challenge_GET(IServiceProvider services)
        {
            this.services = services;
            ((IChallengeRepository)this.services.GetService(typeof(IChallengeRepository))).SetupTable();
        }

        public APIGatewayProxyResponse Challenges_GETHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            request.QueryStringParameters.TryGetValue("username", out string username);

            if (string.IsNullOrEmpty(username))
            {
                return ErrorResponse();
            }

            var challengeRepository = (IChallengeRepository)services.GetService(typeof(IChallengeRepository));

            var userChallenges = challengeRepository.Load(username);

            if(userChallenges == null)
            {
                return Response(new List<IndividualChallenge>());
            }

            RemoveExpiredChallenges(userChallenges);

            challengeRepository.Save(userChallenges);

            return Response(userChallenges.Challenges);
        }

        private void RemoveExpiredChallenges(Challenge challenges)
        {
            var toRemove = new List<IndividualChallenge>();

            foreach(IndividualChallenge challenge in challenges.Challenges)
            {
                if (challenge.Expires < DateTime.Now)
                {
                    toRemove.Add(challenge);
                }
            }

            foreach (IndividualChallenge challenge in toRemove)
            {
                challenges.Challenges.Remove(challenge);
            }
        }

        private APIGatewayProxyResponse Response(List<IndividualChallenge> challenges)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(challenges),
            };
        }

        private APIGatewayProxyResponse ErrorResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"Error no username supplied\" }",
            };
        }
    }
}
