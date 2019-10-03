using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;

namespace Arcade.Shared.Messages
{
    public class Messages
    {
        [DynamoDBHashKey]
        public string UserName { get; set; }

        public List<Message> Notifications { get; set; }
    }
}
