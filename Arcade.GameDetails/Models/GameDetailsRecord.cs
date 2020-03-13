using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.GameDetails
{
    public class GameDetailsRecord
    {
        [DynamoDBHashKey]
        public string Game { get; set; }

        [DynamoDBRangeKey]
        public string SortKey { get; set; }

        public string DataType { get; set; }
                
        public List<ScoreDetails> Scores { get; set; }

        public List<Setting> Settings { get; set; }

        public List<string> Levels { get; set; }

        public Dictionary<string, int> Ratings { get; set; }

        public double Average { get; set; }

        public int Total { get; set; }

        public int NumberOfRatings { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool Finalised { get; set; }

        public override string ToString()
        {
            return $"{Game} - {DataType}";
        }
    }
}
