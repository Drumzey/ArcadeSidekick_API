using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared
{
    public class UserInformation
    {
        [DynamoDBHashKey]
        public string Username { get; set; }

        public string EmailAddress { get; set; }

        public string Secret { get; set; }

        public bool Verified { get; set; }

        public Dictionary<string, string> Games { get; set; }

        public Dictionary<string, int> Ratings { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
