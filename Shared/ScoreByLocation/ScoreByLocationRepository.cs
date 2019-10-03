using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.ScoreByLocation
{
    public class ScoreByLocationRepository : IScoreByLocationRepository
    {
        private DynamoDBContext dbContext;
        private IEnvironmentVariables environmentVariables;

        public ScoreByLocationRepository()
        {
            environmentVariables = new EnvironmentVariables();
        }

        public ScoreByLocationRepository(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(ScoreByLocation)] =
                new Amazon.Util.TypeMapping(typeof(ScoreByLocation), environmentVariables.ScoreByLocationTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this.dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(ScoreByLocation score)
        {
            dbContext.SaveAsync(score).Wait();
        }

        public ScoreByLocation Load(string location)
        {
            return dbContext.LoadAsync<ScoreByLocation>(location).Result;
        }
    }
}
