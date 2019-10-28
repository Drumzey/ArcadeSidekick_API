using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.GameDetails
{
    public class GameRating
    {
        public double Average { get; set; }
        public int NumberOfRatings { get; set; }

        public double WeightedAverage { get; set; }
    }
}
