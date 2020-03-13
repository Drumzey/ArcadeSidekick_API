using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared
{
    public class SingleRatingInformationResponse
    {
        public double Average { get; set; }

        public int NumberOfRatings { get; set; }

        public double WeightedAverage { get; set; }
    }
}
