using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared
{
    public class GameInformation
    {
        [DynamoDBHashKey]
        public string Category { get; set; }

        public List<string> Games { get; set; }

        public List<string> TweetedGames { get; set; }
    }
}