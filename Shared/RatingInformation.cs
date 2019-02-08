using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared
{
    public class RatingInformation
    {
        [DynamoDBHashKey]
        public string GameName { get; set; }

        public Dictionary<string, int> Ratings { get; set; }

        public double Average { get; set; }

        public int Total { get; set; }

        public int NumberOfRatings { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
