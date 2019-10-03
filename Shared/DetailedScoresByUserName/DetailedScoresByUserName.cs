using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared.DetailedScoresByUserName
{
    public class DetailedScoresByUserName
    {
        // Partitian Key
        public string UserName { get; set; }

        // Sort Key
        public string GameName { get; set; }

        // Key is Location
        public Dictionary<string, List<ScoreDetails>> Scores { get; set; }
    }
}
