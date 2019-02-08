using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared.Repositories
{
    public class RatingInformationRepository : IRatingRepository
    {
        private DynamoDBContext _dbContext;
        private IEnvironmentVariables _environmentVariables;

        public RatingInformationRepository()
        {
            _environmentVariables = new EnvironmentVariables();
        }

        public RatingInformationRepository(IEnvironmentVariables environmentVariables)
        {
            _environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(RatingInformation)] = 
                new Amazon.Util.TypeMapping(typeof(RatingInformation), _environmentVariables.RatingInformationTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this._dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(RatingInformation key)
        {
            _dbContext.SaveAsync(key).Wait();
        }

        public RatingInformation Load(string partitionKey)
        {
            return _dbContext.LoadAsync<RatingInformation>(partitionKey).Result;
        }
    }
}
