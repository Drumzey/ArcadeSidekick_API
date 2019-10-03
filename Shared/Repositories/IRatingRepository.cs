using System.Collections.Generic;

namespace Arcade.Shared.Repositories
{
    public interface IRatingRepository
    {
        void SetupTable();

        void Save(RatingInformation key);

        RatingInformation Load(string partitionKey);

        IEnumerable<RatingInformation> AllRows();
    }
}
