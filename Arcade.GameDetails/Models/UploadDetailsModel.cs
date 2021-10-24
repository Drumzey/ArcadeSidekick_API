using System;
using System.Collections.Generic;

namespace Arcade.GameDetails.Models
{
    public class UploadDetailsModel
    {
        public string UserName { get; set; }

        public Dictionary<string, int> Ratings { get; set; }

        public Dictionary<string, string> SimpleScores { get; set; }

        public Dictionary<string, List<DetailedGame>> DetailedScores { get; set; }
    }

    public class DetailedGame
    {
        public string Score { get; set; }

        public string Date { get; set; }

        public string LevelName { get; set; }

        public string EventName { get; set; }

        public string Location { get; set; }

        public string Difficulty { get; set; }

        public string Lives { get; set; }

        public string ExtraLivesAt { get; set; }

        public string MameOrPCB { get; set; }

        public string Credits { get; set; }
    }
}
