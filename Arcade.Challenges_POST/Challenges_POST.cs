using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SimpleEmail.Model;
using Arcade.Shared;
using Arcade.Shared.Messages;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;
using Message = Arcade.Shared.Messages.Message;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.Challenges_POST
{
    public class Challenges_POST
    {
        private IServiceProvider services;

        public Challenges_POST()
            : this(DI.Container.Services())
        {
        }

        public Challenges_POST(IServiceProvider services)
        {
            this.services = services;            
            ((IClubRepository)this.services.GetService(typeof(IClubRepository))).SetupTable();
            ((IMessageRepository)this.services.GetService(typeof(IMessageRepository))).SetupTable();
        }

        public APIGatewayProxyResponse Challenges_POSTHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var data = JsonConvert.DeserializeObject<ChallengePost>(request.Body);

            if (!ValidRequest(data))
            {
                return ErrorResponse();
            }

            if (data.ToClubMembers)
            {
                Console.WriteLine("Sending Club Challenge");
                GenerateClubChallenge(data);
            }
            else
            {
                Console.WriteLine("Sending Friend Challenge");
                GenerateChallenge(data);
            }

            return Response();
        }

        private bool ValidRequest(ChallengePost data)
        {
            if(string.IsNullOrWhiteSpace(data.From))
            {
                return false;
            }

            if(string.IsNullOrWhiteSpace(data.GameName))
            {
                return false;
            }

            if(string.IsNullOrWhiteSpace(data.To))
            {
                return false;
            }

            return true;
        }

        private List<string> GenerateChallenge(ChallengePost data)
        {
            Arcade.Shared.Messages.CreateMessage.Create(
                       services,
                       data.To,
                       "Arcade Sidekick",
                       data.Message,
                       Shared.Messages.MessageTypeEnum.ChallengeReceived,
                       new Dictionary<string, string>
                       {
                            { "Game", data.GameName },
                            { "Challenger", data.From },
                       });

            return new List<string> { data.To };
        }

        private List<string> GenerateClubChallenge(ChallengePost data)
        {
            var clubRepository = (IClubRepository)services.GetService(typeof(IClubRepository));
            var clubInformation = clubRepository.Load(data.From);
            if (clubInformation == null)
            {
                throw new Exception("No club found with given name " + data.From);
            }

            List<string> userList = clubInformation.Members;
            var messages = new List<Messages>();
            var messageRepo = (IMessageRepository)services.GetService(typeof(IMessageRepository));

            Console.WriteLine("Loading all messages");
            var userRecords = messageRepo.BatchGet(userList);
            Console.WriteLine("Loading all loaded");

            var newMessage = new Message
            {
                From = data.From,
                Seen = false,
                TimeSet = DateTime.Now,
                Text = data.Message,
                MessageType = Shared.Messages.MessageTypeEnum.ClubChallengeReceived,
                Data = new Dictionary<string, string>
                       {
                            { "Game", data.GameName },
                            { "Club", data.From }
                       },
            };

            foreach (string user in userList)
            {
                var userRecord = userRecords.Where(x => x.UserName == user).FirstOrDefault();

                Console.WriteLine($"Sending message to {user}");
                messages.Add(Arcade.Shared.Messages.CreateMessage.CreateWithoutPost(
                    userRecord,
                    newMessage,
                    user));
            }

            Console.WriteLine("Sending message to all club members");
            messageRepo.SaveBatch(messages);
            Console.WriteLine("Messages saved");
            return userList;
        }

        private APIGatewayProxyResponse Response()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "{ \"message\": \"Challenges Sent\" }",
            };
        }

        private APIGatewayProxyResponse ErrorResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": \"Error challenge details incomplete\" }",
            };
        }
    }
}
