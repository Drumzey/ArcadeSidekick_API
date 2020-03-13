using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.GameDetails
{
    public class Scores
    {
        public Dictionary<string, List<SimpleScore>> SimpleScores { get; set; }

        public Dictionary<string, Setting> Setting { get; set; }
    }
}
