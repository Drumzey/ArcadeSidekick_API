using System;
using System.Collections.Generic;
using System.Linq;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

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

        public List<Messages> All()
        {
            var scanConditions = new List<ScanCondition>();
            return dbContext.ScanAsync<Messages>(scanConditions).GetNextSetAsync().Result;
        }

        public List<Messages> BatchGet(List<string> userNames)
        {
            Console.WriteLine("creating batch get");
            var batch = dbContext.CreateBatchGet<Messages>();
            var results = new List<Messages>();

            int batchsize = 100;
            Console.WriteLine($"Getting info for {userNames.Count} users");
            Console.WriteLine("batch size 100");
            for (int x = 0; x < Math.Ceiling((decimal)userNames.Count / batchsize); x++)
            {
                Console.WriteLine($"working on batch {x}");
                var userNamesBatch = userNames.Skip(x * batchsize).Take(batchsize);

                foreach (var user in userNamesBatch)
                {
                    batch.AddKey(user);
                }

                Console.WriteLine($"Getting batch {x}");
                batch.ExecuteAsync().Wait();
                Console.WriteLine($"Got batch {x}");
                results.AddRange(batch.Results);
            }

            Console.WriteLine($"finished get batch");
            return results;
        }
    }
}
