using System.Collections.Generic;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.Repositories
{
    public class RatingInformationRepository : IRatingRepository
    {
        private DynamoDBContext dbContext;
        private IEnvironmentVariables environmentVariables;

        public RatingInformationRepository()
        {
            environmentVariables = new EnvironmentVariables();
        }

        public RatingInformationRepository(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(RatingInformation)] =
                new Amazon.Util.TypeMapping(typeof(RatingInformation), environmentVariables.RatingInformationTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this.dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(RatingInformation key)
        {
            dbContext.SaveAsync(key).Wait();
        }

        public RatingInformation Load(string partitionKey)
        {
            return dbContext.LoadAsync<RatingInformation>(partitionKey).Result;
        }

        public IEnumerable<RatingInformation> AllRows()
        {
            var conditions = new List<ScanCondition>();
            var allDocs = dbContext.ScanAsync<RatingInformation>(conditions).GetRemainingAsync().Result;
            return allDocs;
        }
    }
}
