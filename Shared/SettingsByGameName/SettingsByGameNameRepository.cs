using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.SettingsByGameName
{
    public class SettingsByGameNameRepository : ISettingsByGameNameRepository
    {
        private DynamoDBContext dbContext;
        private IEnvironmentVariables environmentVariables;

        public SettingsByGameNameRepository()
        {
            environmentVariables = new EnvironmentVariables();
        }

        public SettingsByGameNameRepository(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(SettingsByGameName)] =
                new Amazon.Util.TypeMapping(typeof(SettingsByGameName), environmentVariables.SettingsByGameNameTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this.dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(SettingsByGameName settings)
        {
            dbContext.SaveAsync(settings).Wait();
        }

        public SettingsByGameName Load(string gamename)
        {
            return dbContext.LoadAsync<SettingsByGameName>(gamename).Result;
        }
    }
}
