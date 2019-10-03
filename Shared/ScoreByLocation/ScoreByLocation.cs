using System.Collections.Generic;
using Arcade.Shared.Shared;

namespace Arcade.Shared.ScoreByLocation
{
    public class ScoreByLocation
    {
        public string Location { get; set; }

        public string GameName { get; set; }

        public Dictionary<string, SimpleScore> Scores { get; set; }
    }
}
