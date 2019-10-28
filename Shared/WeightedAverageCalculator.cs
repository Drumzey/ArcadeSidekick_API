using System;

namespace Arcade.Shared
{
    public static class WeightedAverageCalculator
    {
        public static double CalculateAverage(double average, double votes, double mean)
        {
            var minimumVotes = 5;
            var weightedAverage = (votes / (votes + minimumVotes)) * average + (minimumVotes / (votes + minimumVotes)) * mean;
            Console.WriteLine(weightedAverage);
            return Math.Round(weightedAverage, 2, MidpointRounding.AwayFromZero);
        }
    }
}
