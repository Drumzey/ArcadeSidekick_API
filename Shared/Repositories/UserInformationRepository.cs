using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared.Repositories
{
    public class UserInformationRepository : IUserRepository
    {
        private DynamoDBContext _dbContext;
        private IEnvironmentVariables _environmentVariables;

        public UserInformationRepository()
        {
            _environmentVariables = new EnvironmentVariables();
        }

        public UserInformationRepository(IEnvironmentVariables environmentVariables)
        {
            _environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(UserInformation)] = 
                new Amazon.Util.TypeMapping(typeof(UserInformation), _environmentVariables.UserInformationTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this._dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(UserInformation key)
        {
            _dbContext.SaveAsync(key).Wait();
        }

        public UserInformation Load(string partitionKey)
        {
            return _dbContext.LoadAsync<UserInformation>(partitionKey).Result;
        }
    }
}
