using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared.Repositories
{
    public interface IClubRepository
    {
        void SetupTable();

        void Save(ClubInformation key);

        ClubInformation Load(string partitionKey);
    }
}
