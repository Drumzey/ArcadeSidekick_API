using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.SaveRating
{
    internal class UpdateUserRatingsResponse
    {
        public int NumberOfRatings { get; set; }

        public List<RatingInformation> Ratings { get; set; }
    }

    public class SaveRating
    {
        private IServiceProvider services;

        public SaveRating()
            : this(DI.Container.Services())
        {
        }

        public SaveRating(IServiceProvider services)
        {
            this.services = services;
            ((IRatingRepository)this.services.GetService(typeof(IRatingRepository))).SetupTable();
            ((IUserRepository)this.services.GetService(typeof(IUserRepository))).SetupTable();
            ((IObjectRepository)this.services.GetService(typeof(IObjectRepository))).SetupTable();
        }

        public APIGatewayProxyResponse SaveRatingHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var ratingInfo = SaveRatingInfo(request.Body, request.Headers["Authorization"]);
            SaveIntoObjectTable(ratingInfo);
            return Response(ratingInfo);
        }

        private void SaveIntoObjectTable(UpdateUserRatingsResponse ratingInfo)
        {
            var ratingFromObjectTable = ((IObjectRepository)services.GetService(typeof(IObjectRepository)))
                    .Load("ratings");

            if (ratingFromObjectTable == null)
            {
                ratingFromObjectTable = new ObjectInformation();
                ratingFromObjectTable.Key = "ratings";
                ratingFromObjectTable.DictionaryValue = new Dictionary<string, string>();
            }

            foreach (RatingInformation rating in ratingInfo.Ratings)
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

            ((IObjectRepository)services.GetService(typeof(IObjectRepository))).Save(ratingFromObjectTable);
        }

        private UpdateUserRatingsResponse SaveRatingInfo(string requestBody, string token)
        {
            var data = JsonConvert.DeserializeObject<SaveRatingInformation>(requestBody);
            var ratinginfo = new List<RatingInformation>();

            ValidateJwt(token, data);

            var ratingRepository = (IRatingRepository)services.GetService(typeof(IRatingRepository));
            var userRepository = (IUserRepository)services.GetService(typeof(IUserRepository));
            var user = userRepository.Load(data.Username);

            foreach (SaveSingleRatingInformationInput input in data.Ratings)
            {
                var ratingInformationForGame = ratingRepository.Load(input.GameName);

                if (ratingInformationForGame == null)
                {
                    ratingInformationForGame = new RatingInformation
                    {
                        GameName = input.GameName,
                        NumberOfRatings = 1,
                        Average = input.Rating,
                        Total = input.Rating,
                        CreatedAt = DateTime.Now,
                        Ratings = new Dictionary<string, int>
                        {
                            { data.Username, input.Rating },
                        },
                    };
                }
                else
                {
                    if (ratingInformationForGame.Ratings.ContainsKey(data.Username))
                    {
                        var oldRating = ratingInformationForGame.Ratings[data.Username];
                        var newRating = input.Rating;

                        if (oldRating != newRating)
                        {
                            ratingInformationForGame.Ratings[data.Username] = newRating;
                            ratingInformationForGame.Total += newRating - oldRating;
                        }
                    }
                    else
                    {
                        ratingInformationForGame.Ratings.Add(data.Username, input.Rating);
                        ratingInformationForGame.NumberOfRatings++;
                        ratingInformationForGame.Total += input.Rating;
                    }

                    var average = (double)ratingInformationForGame.Total / ratingInformationForGame.NumberOfRatings;

                    ratingInformationForGame.Average = Math.Round(average, 2);
                }

                ratingInformationForGame.UpdatedAt = DateTime.Now;
                ratingRepository.Save(ratingInformationForGame);
                ratinginfo.Add(ratingInformationForGame);

                UpdateUserRepository(user, input.GameName, input.Rating);
            }

            user.NumberOfRatingsGiven = user.Ratings.Where(x => x.Value != 0).Count();
            userRepository.Save(user);

            return new UpdateUserRatingsResponse
            {
                NumberOfRatings = user.NumberOfRatingsGiven,
                Ratings = ratinginfo,
            };
        }

        private static void ValidateJwt(string token, SaveRatingInformation data)
        {
            JwtSecurityToken jwtToken;
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            try
            {
                jwtToken = handler.ReadJwtToken(token);
            }
            catch
            {
                throw new Exception("Invalid JWT");
            }

            if (jwtToken.Id.ToLower() != data.Username.ToLower())
            {
                throw new Exception("Attempting to Save data for other user");
            }
        }

        private void UpdateUserRepository(UserInformation user, string gameName, int rating)
        {
            user.Ratings[gameName] = rating;
            if (!user.Games.ContainsKey(gameName))
            {
                user.Games[gameName] = "0";
            }
        }

        private APIGatewayProxyResponse Response(UpdateUserRatingsResponse ratingInfo)
        {
            var response = new SaveRatingInformationResponse();
            response.Games = new Dictionary<string, SingleRatingInformationResponse>();

            foreach (RatingInformation info in ratingInfo.Ratings)
            {
                response.Games.Add(info.GameName, new SingleRatingInformationResponse
                {
                    Average = info.Average,
                    NumberOfRatings = info.NumberOfRatings,
                });
            }

            response.NumberOfRatingsForUser = ratingInfo.NumberOfRatings;

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(response),
            };
        }
    }
}
