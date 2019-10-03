using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.ListItems
{
    public class ListItemsRepository : IListItemsRepository
    {
        private DynamoDBContext dbContext;
        private IEnvironmentVariables environmentVariables;

        public ListItemsRepository()
        {
            environmentVariables = new EnvironmentVariables();
        }

        public ListItemsRepository(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(ListItems)] =
                new Amazon.Util.TypeMapping(typeof(ListItems), environmentVariables.ListItemsTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this.dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(ListItems item)
        {
            dbContext.SaveAsync(item).Wait();
        }

        public ListItems Load(string key)
        {
            return dbContext.LoadAsync<ListItems>(key).Result;
        }
    }
}
