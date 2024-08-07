﻿using System.Collections.Generic;
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

        public List<string> Events { get; set; }

        public string CurrentCompetition { get; set; }

        public string Secret { get; set; }

        public string Link { get; set; }

        public int MessagesSent { get; set; }

        public string TwitterHandle { get; set; }

        public string Facebook { get; set; }

        public string Instagram { get; set; }

        public string Type { get; set; }
    }
}
