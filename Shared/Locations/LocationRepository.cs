using System.Collections.Generic;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.Locations
{
    public class LocationRepository : ILocationRepository
    {
        private DynamoDBContext dbContext;
        private IEnvironmentVariables environmentVariables;

        public LocationRepository()
        {
            environmentVariables = new EnvironmentVariables();
        }

        public LocationRepository(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(Location)] =
                new Amazon.Util.TypeMapping(typeof(Location), environmentVariables.LocationTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this.dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(Location location)
        {
            dbContext.SaveAsync(location).Wait();
        }

        public Location Load(string locationName)
        {
            return dbContext.LoadAsync<Location>(locationName).Result;
        }

        public List<Location> AllLocations()
        {
            var conditions = new List<ScanCondition>();
            var allLocs = dbContext.ScanAsync<Location>(conditions).GetRemainingAsync().Result;
            return allLocs;
        }
    }
}
