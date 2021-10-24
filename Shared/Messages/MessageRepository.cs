using System.Collections.Generic;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

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

        public void SaveBatch(List<Messages> messages)
        {
            var batch = dbContext.CreateBatchWrite<Messages>();
            batch.AddPutItems(messages);
            batch.ExecuteAsync();
        }

        public void Save(Messages message)
        {
            dbContext.SaveAsync(message).Wait();
        }

        public Messages Load(string username)
        {
            return dbContext.LoadAsync<Messages>(username).Result;
        }

        public List<Messages> Scan(IEnumerable<ScanCondition> scanConditions)
        {
            return dbContext.ScanAsync<Messages>(scanConditions).GetNextSetAsync().Result;
        }
    }
}
