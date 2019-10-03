using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Repositories;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.RatingMigration
{
    public class RatingMigration
    {

        private IServiceProvider _services;

        public RatingMigration()
            : this(DI.Container.Services())
        {
        }

        public RatingMigration(IServiceProvider services)
        {
            _services = services;
            ((IRatingRepository)_services.GetService(typeof(IRatingRepository))).SetupTable();
            ((IObjectRepository)_services.GetService(typeof(IObjectRepository))).SetupTable();
        }

        public APIGatewayProxyResponse RatingMigrationHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            //Do the scan
            var ratings = ((IRatingRepository)_services.GetService(typeof(IRatingRepository))).AllRows();

            //Get the value from the object table
            var ratingFromObjectTable = ((IObjectRepository)_services.GetService(typeof(IObjectRepository)))
                    .Load("ratings");

            if (ratingFromObjectTable == null)
            {
                ratingFromObjectTable = new ObjectInformation();
                ratingFromObjectTable.Key = "ratings";
                ratingFromObjectTable.DictionaryValue = new Dictionary<string, string>();                
            }

            foreach (RatingInformation rating in ratings)
            {
                if (!ratingFromObjectTable.DictionaryValue.ContainsKey(rating.GameName))
                {
                    ratingFromObjectTable.DictionaryValue.Add(rating.GameName, rating.Average.ToString());
                }
                else
                {
                    ratingFromObjectTable.DictionaryValue[rating.GameName] = rating.Average.ToString();
                }
            }

            ((IObjectRepository)_services.GetService(typeof(IObjectRepository))).Save(ratingFromObjectTable);
            
            return Response();
        }

        private APIGatewayProxyResponse Response()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "{ \"message\": \"Done\"}",
            };
        }

    }
}
