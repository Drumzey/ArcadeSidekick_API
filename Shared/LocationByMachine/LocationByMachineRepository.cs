using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.LocationByMachine
{
    public class LocationByMachineRepository : ILocationByMachineRepository
    {
        private DynamoDBContext dbContext;
        private IEnvironmentVariables environmentVariables;

        public LocationByMachineRepository()
        {
            environmentVariables = new EnvironmentVariables();
        }

        public LocationByMachineRepository(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(LocationByMachine)] =
                new Amazon.Util.TypeMapping(typeof(LocationByMachine), environmentVariables.LocationByMachineTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this.dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(LocationByMachine location)
        {
            dbContext.SaveAsync(location).Wait();
        }

        public LocationByMachine Load(string gamename)
        {
            return dbContext.LoadAsync<LocationByMachine>(gamename).Result;
        }
    }
}
