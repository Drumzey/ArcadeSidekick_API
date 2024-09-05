using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Arcade.Shared;
using Arcade.Shared.Misc;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.GetUsers
{
    public class GetUsers
    {
        private IServiceProvider services;
        private IEnvironmentVariables environmentVariables;

        public GetUsers()
            : this(DI.Container.Services())
        {
        }

        public GetUsers(IServiceProvider services)
        {
            this.services = services;
            ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).SetupTable();
            environmentVariables = (IEnvironmentVariables)this.services.GetService(typeof(IEnvironmentVariables));
        }

        public APIGatewayProxyResponse GetUsersHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                // Try and get the list of all users from the temp cache
                var usersFromS3 = this.ReadS3Object();

                if (usersFromS3 == null)
                {
                    var users = ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Load("Activity", "Users");
                    Console.WriteLine("Got User results from Origin");
                    this.WriteUserListToS3(users.List1);
                    return OkResponse(users.List1);
                }
                else
                {
                    Console.WriteLine("Got User results from Cache");
                    return OkResponse(usersFromS3, "Cache");
                }
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        private APIGatewayProxyResponse OkResponse(List<string> users, string cacheHeader = null)
        {
            var headers = new Dictionary<string, string>();
            headers.Add("Origin", cacheHeader ?? "Origin");

            return new APIGatewayProxyResponse
            {
                Headers = headers,
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(users),
            };
        }

        private APIGatewayProxyResponse ErrorResponse(string error)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = "{ \"message\": \"Error. " + error + "\"}",
            };
        }

        private List<string> ReadS3Object()
        {
            AmazonS3Client client;

            using (client = new AmazonS3Client(Amazon.RegionEndpoint.EUWest2))
            {
                try
                {
                    GetObjectRequest request = new GetObjectRequest();
                    request.BucketName = "arcadesidekick";
                    request.Key = "API_TEMP/USERS.txt";
                    GetObjectResponse response = client.GetObjectAsync(request).Result;
                    StreamReader reader = new StreamReader(response.ResponseStream);
                    var content = reader.ReadToEnd();
                    string[] lines = content.Split(
                        new string[] { "\r\n", "\r", "\n" },
                        StringSplitOptions.None
                    );
                    return lines.ToList();
                }
                catch(Exception e)
                {
                    return null;
                }
            }
        }

        private void WriteUserListToS3(List<string> users)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var user in users)
            {
                if (user == users.Last())
                {
                    builder.Append(user);
                }
                else
                {
                    builder.AppendLine(user);
                }
            }

            AmazonS3Client client;

            using (client = new AmazonS3Client(
                environmentVariables.AWSAccessKey,
                environmentVariables.AWSAccessKeySecret,
                Amazon.RegionEndpoint.EUWest2))
            {
                try
                {
                    PutObjectRequest putObjectRequest = new PutObjectRequest();
                    putObjectRequest.BucketName = "arcadesidekick";
                    putObjectRequest.Key = "API_TEMP/USERS.txt";
                    putObjectRequest.ContentBody = builder.ToString();
                    putObjectRequest.ContentType = "text/plain";

                    var response = client.PutObjectAsync(putObjectRequest).Result;

                    if (response.HttpStatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine("Written to user Cache");
                    }
                    else
                    {
                        Console.WriteLine($"Have not written to user cache: {response.HttpStatusCode}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
