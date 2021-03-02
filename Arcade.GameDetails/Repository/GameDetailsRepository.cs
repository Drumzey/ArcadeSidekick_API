using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Arcade.Shared;
using System;
using System.Collections.Generic;

namespace Arcade.GameDetails
{
    public class GameDetailsRepository : IGameDetailsRepository
    {
        private DynamoDBContext dbContext;
        private IEnvironmentVariables environmentVariables;

        public GameDetailsRepository()
        {
            environmentVariables = new EnvironmentVariables();
        }

        public GameDetailsRepository(IEnvironmentVariables environmentVariables)
        {
            this.environmentVariables = environmentVariables;
        }

        public void SetupTable()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(GameDetailsRecord)] =
                new Amazon.Util.TypeMapping(typeof(GameDetailsRecord), environmentVariables.GameDetailsTableName);

            var config = new DynamoDBContextConfig { ConsistentRead = true, Conversion = DynamoDBEntryConversion.V2 };
            this.dbContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        public void Save(GameDetailsRecord record)
        {
            dbContext.SaveAsync(record).Wait();
        }

        public GameDetailsRecord Load(string gameName, string location)
        {
            return dbContext.LoadAsync<GameDetailsRecord>(gameName, location).Result;
        }

        public List<GameDetailsRecord> QueryByGameName(string gameName)
        {
            return dbContext.QueryAsync<GameDetailsRecord>(gameName).GetNextSetAsync().Result;
        }

        public IEnumerable<GameDetailsRecord> AllGamesForUser(string userName)
        {
            var conditions = new List<ScanCondition>();
            var condition1 = new ScanCondition(
                "SortKey", 
                Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal,
                new object[1] { userName });
            conditions.Add(condition1);

            Console.WriteLine("CONDITION: " + condition1.ToString());

            var allDocs = dbContext.ScanAsync<GameDetailsRecord>(conditions).GetRemainingAsync().Result;
            return allDocs;
        }

        public IEnumerable<GameDetailsRecord> AllRows()
        {
            var conditions = new List<ScanCondition>();
            var allDocs = dbContext.ScanAsync<GameDetailsRecord>(conditions).GetRemainingAsync().Result;
            return allDocs;
        }

        public void Delete(string gameName, string sortKey)
        {
            dbContext.DeleteAsync<GameDetailsRecord>(gameName, sortKey);
        }
    }
}
