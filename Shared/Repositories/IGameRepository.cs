using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared.Repositories
{
    public interface IGameRepository
    {
        void SetupTable();

        void Save(GameInformation key);

        GameInformation Load(string partitionKey);
    }
}
