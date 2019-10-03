using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.ScoreByGameName
{
    public class ScoreByGameNameRepository : IScoreByGameNameRepository
    {
        private DynamoDBContext dbContext;
        private IEnvironmentVariables environmentVariables;

        public ScoreByGameNameRepository()
        {
            environmentVariables = new EnvironmentVariables();
        }

        public ScoreByGameNameRepository(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(ScoreByGameName)] =
                new Amazon.Util.TypeMapping(typeof(ScoreByGameName), environmentVariables.ScoreByGameNameTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this.dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(ScoreByGameName score)
        {
            dbContext.SaveAsync(score).Wait();
        }

        public ScoreByGameName Load(string gamename)
        {
            return dbContext.LoadAsync<ScoreByGameName>(gamename).Result;
        }
    }
}
