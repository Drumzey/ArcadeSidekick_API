using System.Collections.Generic;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.Misc
{
    public class MiscRepository : IMiscRepository
    {
        private DynamoDBContext dbContext;
        private IEnvironmentVariables environmentVariables;

        public MiscRepository()
        {
            environmentVariables = new EnvironmentVariables();
        }

        public MiscRepository(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(Misc)] =
                new Amazon.Util.TypeMapping(typeof(Misc), environmentVariables.MiscTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this.dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(Misc misc)
        {
            dbContext.SaveAsync(misc).Wait();
        }

        public Misc Load(string key, string sortKey)
        {
            return dbContext.LoadAsync<Misc>(key, sortKey).Result;
        }

        public List<Misc> QueryByFollowerName(string key)
        {
            return dbContext.QueryAsync<Misc>(key).GetNextSetAsync().Result;
        }
    }
}
