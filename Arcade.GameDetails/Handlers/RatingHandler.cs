using Arcade.Shared;
using Arcade.Shared.Misc;

namespace Arcade.GameDetails.Handlers
{
    public class RatingHandler
    {
        /// <summary>
        /// Returns the weighted average of a game
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        public GameRating Get(IMiscRepository repo, GameDetailsRecord details)
        {
            if (details == null)
            {
                return new GameRating
                {
                    Average = 0,
                    NumberOfRatings = 0,
                    WeightedAverage = 0,
                };
            };

            var average = details.Average;
            var ratings = details.NumberOfRatings;
            var averageOfAllGamesRecord = repo.Load("Ratings", "Average");
            var averageOfAllGames = double.Parse(averageOfAllGamesRecord.Value);

            var weightedAverage = WeightedAverageCalculator.CalculateAverage(average, (double)ratings, averageOfAllGames);

            return new GameRating
            {
                Average = details.Average,
                NumberOfRatings = details.NumberOfRatings,
                WeightedAverage = weightedAverage,
            };
        }

        public GameRating Get(IGameDetailsRepository repo, IMiscRepository miscRepo, string gameName)
        {
            var details = repo.Load(gameName, "Rating");
            return Get(miscRepo, details);
        }
    }
}
