using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared.Repositories
{
    public interface IObjectRepository
    {
        void SetupTable();

        void Save(ObjectInformation key);

        ObjectInformation Load(string partitionKey);
    }
}
