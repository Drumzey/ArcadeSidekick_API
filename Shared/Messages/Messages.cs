using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.Messages
{
    public class Messages
    {
        [DynamoDBHashKey]
        public string UserName { get; set; }

        public List<Message> Notifications { get; set; }
    }
}
