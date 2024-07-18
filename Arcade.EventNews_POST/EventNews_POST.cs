using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared.Messages;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.EventNews_POST
{
    public class EventNews_POST
    {
        private IServiceProvider services;

        public EventNews_POST()
            : this(DI.Container.Services())
        {
        }

        public EventNews_POST(IServiceProvider services)
        {
            this.services = services;
            ((IMessageRepository)this.services.GetService(typeof(IMessageRepository))).SetupTable();
            ((IClubRepository)this.services.GetService(typeof(IClubRepository))).SetupTable();
            ((IUserRepository)this.services.GetService(typeof(IUserRepository))).SetupTable();
        }

        public APIGatewayProxyResponse EventNews_POSTHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var data = JsonConvert.DeserializeObject<EventNewsPost>(request.Body);

            if (!ValidRequest(data))
            {
                return ErrorResponse("Invalid input data");
            }

            var clubRepository = (IClubRepository)services.GetService(typeof(IClubRepository));
            var clubInformation = clubRepository.Load(data.From);
            if (clubInformation == null)
            {
                return ErrorResponse("No club found with given name " + data.From);
            }

            List<string> userList;

            if (data.Type == "Invite")
            {
                userList = data.InviteList.Split(',').ToList();
            }
            else
            {
                userList = clubInformation.Members.Union(clubInformation.AdminUsers).ToList();
            }

            GenerateEventNews(data, userList);

            clubInformation.MessagesSent += userList.Count();
            clubRepository.Save(clubInformation);

            return Response();
        }

        private bool ValidRequest(EventNewsPost data)
        {
            if (string.IsNullOrWhiteSpace(data.From))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(data.Message))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(data.Type))
            {
                return false;
            }

            if (data.Type == "Invite" && string.IsNullOrEmpty(data.InviteList))
            {
                return false;
            }

            return true;
        }

        private void GenerateEventNews(EventNewsPost data, List<string> userList)
        {
            MessageTypeEnum messagetype = GetMessageType(data);
            var messages = new List<Messages>();
            var messageRepo = (IMessageRepository)services.GetService(typeof(IMessageRepository));

            foreach (string user in userList)
            {
                Console.WriteLine($"Sending message to {user}");
                messages.Add(Arcade.Shared.Messages.CreateMessage.CreateWithoutPost(
                       messageRepo,
                       user,
                       data.From,
                       data.Message,
                       messagetype,
                       new Dictionary<string, string>
                       {
                            { "Club", data.From }
                       }));
            }

            Console.WriteLine("Sending message to all club members");
            messageRepo.SaveBatch(messages);
            Console.WriteLine("Messages saved");
        }

        private static MessageTypeEnum GetMessageType(EventNewsPost data)
        {
            MessageTypeEnum messagetype;
            switch (data.Type)
            {
                case "Event":
                    messagetype = Shared.Messages.MessageTypeEnum.ClubEvent;
                    break;
                case "News":
                    messagetype = Shared.Messages.MessageTypeEnum.ClubNews;
                    break;
                case "Invite":
                    messagetype = Shared.Messages.MessageTypeEnum.ClubInvite;
                    break;
                default:
                    messagetype = Shared.Messages.MessageTypeEnum.ClubNews;
                    break;
            }

            return messagetype;
        }

        private APIGatewayProxyResponse Response()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "{ \"message\": \"News Sent\" }",
            };
        }

        private APIGatewayProxyResponse ErrorResponse(string message)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{ \"message\": " + message + " }",
            };
        }
    }
}
