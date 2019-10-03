using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.DetailedScoresByUserName
{
    public class DetailedScoresByUserNameRepository : IDetailedScoresByUserNameRepository
    {
        private DynamoDBContext dbContext;
        private IEnvironmentVariables environmentVariables;

        public DetailedScoresByUserNameRepository()
        {
            environmentVariables = new EnvironmentVariables();
        }

        public DetailedScoresByUserNameRepository(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(DetailedScoresByUserName)] =
                new Amazon.Util.TypeMapping(typeof(DetailedScoresByUserName), environmentVariables.DetailedScoresByUserNameTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this.dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(DetailedScoresByUserName score)
        {
            dbContext.SaveAsync(score).Wait();
        }

        public DetailedScoresByUserName Load(string username)
        {
            return dbContext.LoadAsync<DetailedScoresByUserName>(username).Result;
        }
    }
}
