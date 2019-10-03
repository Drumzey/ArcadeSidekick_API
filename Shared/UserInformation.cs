using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

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

        public List<string> Clubs { get; set; }

        public string TwitterHandle { get; set; }

        public string Location { get; set; }

        public int NumberOfGamesPlayed { get; set; }

        public int NumberOfRatingsGiven { get; set; }

        public int NumberOfScoresUploaded { get; set; }

        public int NumberOfSocialShares { get; set; }

        public int NumberOfChallengesSent { get; set; }
    }
}
