using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.PutGamePositionHistory
{
    public class PutGamePositionHistory
    {
        private IServiceProvider services;

        public PutGamePositionHistory()
            : this(DI.Container.Services())
        {
        }

        public PutGamePositionHistory(IServiceProvider services)
        {
            this.services = services;
        }

        public APIGatewayProxyResponse PutGamePositionHistoryHandler(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            var gamePositions = GetAllGameHighscores();

            foreach(var game in gamePositions)
            {
                if(game.Value.Any())
                {
                    // We have some results for this game so we can map them out
                    var content = GetContent(game);
                    var currentDocs = GetS3Object("arcadesidekick", "data", game.Key);
                    PutS3Object("arcadesidekick", "data", game.Key, content);
                }
            }

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "OK",
            };
        }

        /// <summary>
        /// Method to retrieve the current positions of all players
        /// on every game available in the system.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<string>> GetAllGameHighscores()
        {

        }

        /// <summary>
        /// Method to construct the content of a new game position history
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private string GetContent(KeyValuePair<string, List<string>> results)
        {

        }

        /// <summary>
        /// Get the current object from S3, convert it from JSON to the History Class
        /// and return it
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static async Task<bool> GetS3Object(string bucket, string key, string filePath)
        {
            try
            {
                using (var client = new AmazonS3Client(Amazon.RegionEndpoint.EUWest2))
                {
                    var request = new GetObjectRequest
                    {
                        BucketName = bucket,
                        Key = key,
                        FilePath = $"{filePath}.json"
                    };
                    var response = await client.GetObjectAsync(request);
                    var reader = new StreamReader(response.ResponseStream);
                    string content = reader.ReadToEnd();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in GetS3Object:" + ex.Message);
                return false;
            }
        }

        private static async Task<bool> PutS3Object(string bucket, string key, string filePath, string content)
        {
            try
            {
                using (var client = new AmazonS3Client(Amazon.RegionEndpoint.EUWest2))
                {
                    var request = new PutObjectRequest
                    {
                        BucketName = bucket,
                        Key = key,
                        FilePath = filePath,
                        ContentBody = content
                    };
                    var response = await client.PutObjectAsync(request);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in PutS3Object:" + ex.Message);
                return false;
            }
        }
    }
}
