using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.Leagues
{
    public class LeagueRepository : ILeagueRepository
    {
        private DynamoDBContext dbContext;
        private IEnvironmentVariables environmentVariables;

        public LeagueRepository()
        {
            environmentVariables = new EnvironmentVariables();
        }

        public LeagueRepository(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(LeagueDetails)] =
                new Amazon.Util.TypeMapping(typeof(LeagueDetails), environmentVariables.LeagueTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this.dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(LeagueDetails details)
        {
            dbContext.SaveAsync(details).Wait();
        }

        public LeagueDetails Load(string leagueName)
        {
            return dbContext.LoadAsync<LeagueDetails>(leagueName).Result;
        }
    }
}
