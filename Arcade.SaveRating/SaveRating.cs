using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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
    public class SaveRating
    {
        private IServiceProvider _services;

        public SaveRating()
            : this(DI.Container.Services())
        {
        }

        public SaveRating(IServiceProvider services)
        {
            _services = services;
            ((IRatingRepository)_services.GetService(typeof(IRatingRepository))).SetupTable();
            ((IUserRepository)_services.GetService(typeof(IUserRepository))).SetupTable();
        }

        public APIGatewayProxyResponse SaveRatingHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var ratingInfo = SaveRatingInfo(request.Body, request.Headers["Authorization"]);
            return Response(ratingInfo);
        }

        private List<RatingInformation> SaveRatingInfo(string requestBody, string token)
        {
            var data = JsonConvert.DeserializeObject<SaveRatingInformation>(requestBody);
            var ratinginfo = new List<RatingInformation>();

            JwtSecurityToken jwtToken;
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            try
            {
                jwtToken = handler.ReadJwtToken(token);
            }
            catch (Exception e)
            {
                throw new Exception("Invalid JWT");
            }

            if (jwtToken.Id.ToLower() != data.Username.ToLower())
            {
                throw new Exception("Attempting to Save data for other user");
            }

            var repository = ((IRatingRepository)_services.GetService(typeof(IRatingRepository)));

            //Loop for each games in the ratings
            foreach (SaveSingleRatingInformationInput input in data.Ratings)
            {
                var ratingInformationForGame = repository.Load(input.GameName);

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
                            { data.Username, input.Rating},
                        }
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
                            ratingInformationForGame.Total += (newRating - oldRating);
                        }
                    }
                    else
                    {
                        ratingInformationForGame.Ratings.Add(data.Username, input.Rating);
                        ratingInformationForGame.NumberOfRatings++;
                        ratingInformationForGame.Total += input.Rating;
                    }

                    ratingInformationForGame.Average = ratingInformationForGame.Total / ratingInformationForGame.NumberOfRatings;
                }

                ratingInformationForGame.UpdatedAt = DateTime.Now;
                repository.Save(ratingInformationForGame);
                ratinginfo.Add(ratingInformationForGame);

                UpdateUserRepository(data.Username, input.GameName, input.Rating);
            }

            return ratinginfo;
        }

        private void UpdateUserRepository(string username, string gameName, int rating)
        {
            Console.WriteLine(username);
            Console.WriteLine(gameName);
            Console.WriteLine(rating);
            var repository = ((IUserRepository)_services.GetService(typeof(IUserRepository)));
            var user = repository.Load(username);
            user.Ratings[gameName] = rating;
            if(!user.Games.ContainsKey(gameName))
            {
                user.Games[gameName] = "0";
            }
            repository.Save(user);
        }

        private APIGatewayProxyResponse Response(List<RatingInformation> ratingInfo)
        {
            var response = new SaveRatingInformationResponse();
            response.Games = new Dictionary<string, SingleRatingInformationResponse>();

            foreach(RatingInformation info in ratingInfo)
            {
                response.Games.Add(info.GameName, new SingleRatingInformationResponse
                {
                    Average = info.Average,
                    NumberOfRatings = info.NumberOfRatings,
                });
            }

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(response),
            };
        }
    }
}
