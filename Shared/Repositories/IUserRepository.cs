using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.Repositories
{
    public interface IUserRepository
    {
        void SetupTable();

        void Save(UserInformation key);

        List<UserInformation> Scan(IEnumerable<ScanCondition> scanConditions);

        UserInformation Load(string partitionKey);
    }
}
