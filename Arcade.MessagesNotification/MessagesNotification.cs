// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.MessagesNotification
{
    public class MessagesNotification
    {
        private IServiceProvider services;

        public MessagesNotification()
            : this(DI.Container.Services())
        {
        }

        public MessagesNotification(IServiceProvider services)
        {
            this.services = services;
            ((IMessageRepository)this.services.GetService(typeof(IMessageRepository))).SetupTable();
        }
        public APIGatewayProxyResponse MessagesNotificationHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            //Get All Messages
            var conditions = new List<ScanCondition>();            
            var messages = ((IMessageRepository)this.services.GetService(typeof(IMessageRepository))).Scan(conditions);

            //For each message see if there are any unread messages
            foreach(var message in messages)
            {
                var unseen = message.Notifications.Where(x => x.Seen == false).ToList();

                if (unseen.Count() > 0)
                {
                    SendNotification(message.UserName, unseen.Count);
                }
            }
        }

        private void SendNotification(string userName, int count)
        {
            
        }
    }
}
