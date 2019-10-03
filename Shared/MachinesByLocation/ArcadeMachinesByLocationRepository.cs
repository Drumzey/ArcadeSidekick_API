using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.MachinesByLocation
{
    public class ArcadeMachinesByLocationRepository : IArcadeMachinesByLocationRepository
    {
        private DynamoDBContext dbContext;
        private IEnvironmentVariables environmentVariables;

        public ArcadeMachinesByLocationRepository()
        {
            environmentVariables = new EnvironmentVariables();
        }

        public ArcadeMachinesByLocationRepository(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(ArcadeMachinesByLocation)] =
                new Amazon.Util.TypeMapping(typeof(ArcadeMachinesByLocation), environmentVariables.ArcadeMachinesByLocationTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this.dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(ArcadeMachinesByLocation machine)
        {
            dbContext.SaveAsync(machine).Wait();
        }

        public ArcadeMachinesByLocation Load(string locationName)
        {
            return dbContext.LoadAsync<ArcadeMachinesByLocation>(locationName).Result;
        }
    }
}
