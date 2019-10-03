using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.Repositories
{
    public class ClubRepository : IClubRepository
    {
        private DynamoDBContext dbContext;
        private IEnvironmentVariables environmentVariables;

        public ClubRepository()
        {
            environmentVariables = new EnvironmentVariables();
        }

        public ClubRepository(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(ClubInformation)] =
                new Amazon.Util.TypeMapping(typeof(ClubInformation), environmentVariables.ClubTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this.dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(ClubInformation key)
        {
            dbContext.SaveAsync(key).Wait();
        }

        public ClubInformation Load(string partitionKey)
        {
            return dbContext.LoadAsync<ClubInformation>(partitionKey).Result;
        }
    }
}
