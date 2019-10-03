using System;
using System.Collections.Generic;
using System.Text;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared
{
    public class ClubInformation
    {
        [DynamoDBHashKey]
        public string Name { get; set; }

        public string ShortDescription { get; set; }

        public string LongDescription { get; set; }

        public List<string> Members { get; set; }

        public List<string> AdminUsers { get; set; }

        public string CurrentCompetition { get; set; }

        public string Secret { get; set; }

        public string Link { get; set; }

        public int MessagesSent { get; set; }
    }
}
