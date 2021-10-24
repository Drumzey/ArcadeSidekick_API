using Arcade.Shared;
using System.Collections.Generic;

namespace Arcade.GameDetails
{
    public class Scores
    {
        public Dictionary<string, List<SimpleScoreWithVerified>> SimpleScores { get; set; }

        public Dictionary<string, Setting> Setting { get; set; }
    }
}
