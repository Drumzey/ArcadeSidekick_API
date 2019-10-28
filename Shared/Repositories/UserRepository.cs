using System.Collections.Generic;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.Repositories
{
    public class UserRepository : IUserRepository
    {
        private DynamoDBContext dbContext;
        private IEnvironmentVariables environmentVariables;

        public UserRepository()
        {
            environmentVariables = new EnvironmentVariables();
        }

        public UserRepository(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(UserInformation)] =
                new Amazon.Util.TypeMapping(typeof(UserInformation), environmentVariables.UserInformationTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this.dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(UserInformation key)
        {
            dbContext.SaveAsync(key).Wait();
        }

        public UserInformation Load(string partitionKey)
        {
            return dbContext.LoadAsync<UserInformation>(partitionKey).Result;
        }

        public List<UserInformation> Scan(IEnumerable<ScanCondition> scanConditions)
        {
            return dbContext.ScanAsync<UserInformation>(scanConditions).GetNextSetAsync().Result;
        }
    }
}
