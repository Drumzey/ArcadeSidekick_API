using System.Collections.Generic;
using Arcade.Shared.Shared;

namespace Arcade.Shared.ScoreByGameName
{
    public class ScoreByGameName
    {
        public string GameName { get; set; }

        public Dictionary<string, SimpleScore> Scores { get; set; }
    }
}
