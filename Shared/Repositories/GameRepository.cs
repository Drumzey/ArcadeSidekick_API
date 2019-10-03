using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.Repositories
{
    public class GameRepository : IGameRepository
    {
        private DynamoDBContext dbContext;
        private IEnvironmentVariables environmentVariables;

        public GameRepository()
        {
            environmentVariables = new EnvironmentVariables();
        }

        public GameRepository(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(GameInformation)] =
                new Amazon.Util.TypeMapping(typeof(GameInformation), environmentVariables.GameTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this.dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(GameInformation key)
        {
            dbContext.SaveAsync(key).Wait();
        }

        public GameInformation Load(string partitionKey)
        {
            return dbContext.LoadAsync<GameInformation>(partitionKey).Result;
        }
    }
}
