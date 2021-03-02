using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared.Messages;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.MyMessages_GET
{
    public class MyMessages_GET
    {
        private IServiceProvider services;

        public MyMessages_GET()
            : this(DI.Container.Services())
        {
        }

        public MyMessages_GET(IServiceProvider services)
        {
            this.services = services;
            ((IMessageRepository)this.services.GetService(typeof(IMessageRepository))).SetupTable();
        }

        public APIGatewayProxyResponse MyMessages_GETHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            request.QueryStringParameters.TryGetValue("username", out string username);

            if (string.IsNullOrEmpty(username))
            {
                return ErrorResponse();
            }

            var messageRepository = (IMessageRepository)services.GetService(typeof(IMessageRepository));

            var userMessages = messageRepository.Load(username);

            if (userMessages == null)
            {
                return Response(new List<Message>(), new List<Message>());
            }

            //Get unseen messages
            var newMessages = userMessages.Notifications.Where(x => x.Seen == false).OrderByDescending(x => x.TimeSet).ToList();

            //Get seen messages
            var oldMessages = userMessages.Notifications.Where(x => x.Seen == true).OrderByDescending(x => x.TimeSet).ToList();

            MarkMessagesAsSeen(request, newMessages);

            messageRepository.Save(userMessages);

            return Response(newMessages, oldMessages);
        }

        private static void MarkMessagesAsSeen(APIGatewayProxyRequest request, List<Message> newMessages)
        {
            request.QueryStringParameters.TryGetValue("clear", out string clear);

            if (clear.Equals("true"))
            {
                //Mark unseen messages as seen.
                foreach (var mess in newMessages)
                {
                    mess.Seen = true;
                }
            }
        }

        private APIGatewayProxyResponse Response(List<Message> newMessages, List<Message> oldMessages)
        {
            var result = new
            {
                New = newMessages,
                Old = oldMessages,
            };

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(result),
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
