using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Arcade.Shared.Messages;

namespace Arcade.Shared.Messages
{
    public class MessageRepository : IMessageRepository
    {
        private DynamoDBContext dbContext;
        private IEnvironmentVariables environmentVariables;

        public MessageRepository()
        {
            environmentVariables = new EnvironmentVariables();
        }

        public MessageRepository(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(Messages)] =
                new Amazon.Util.TypeMapping(typeof(Messages), environmentVariables.MessageTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this.dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(Messages message)
        {
            dbContext.SaveAsync(message).Wait();
        }

        public Messages Load(string username)
        {
            return dbContext.LoadAsync<Messages>(username).Result;
        }
    }
}
