using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared.Repositories
{
    public interface IRatingRepository
    {
        void SetupTable();

        void Save(RatingInformation key);

        RatingInformation Load(string partitionKey);
    }
}
