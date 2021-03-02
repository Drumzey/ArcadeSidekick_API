using System.Collections.Generic;

namespace Arcade.Shared.Repositories
{
    public interface IObjectRepository
    {
        void SetupTable();

        void Save(ObjectInformation key);

        ObjectInformation Load(string partitionKey);

        List<ObjectInformation> AllScores();
    }
}
