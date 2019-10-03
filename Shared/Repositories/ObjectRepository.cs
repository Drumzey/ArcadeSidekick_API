using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.Repositories
{
    public class ObjectRepository : IObjectRepository
    {
        private DynamoDBContext dbContext;
        private IEnvironmentVariables environmentVariables;

        public ObjectRepository()
        {
            environmentVariables = new EnvironmentVariables();
        }

        public ObjectRepository(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(ObjectInformation)] =
                new Amazon.Util.TypeMapping(typeof(ObjectInformation), environmentVariables.ObjectTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this.dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(ObjectInformation key)
        {
            dbContext.SaveAsync(key).Wait();
        }

        public ObjectInformation Load(string partitionKey)
        {
            return dbContext.LoadAsync<ObjectInformation>(partitionKey).Result;
        }
    }
}
