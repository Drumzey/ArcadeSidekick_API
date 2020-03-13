using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.GameDetails;
using Arcade.Shared;
using Arcade.Shared.Misc;
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
        private int totalNumberOfRatingsChangedBy;
        private int totalOfRatingsChangedBy;

        public SaveRating()
            : this(DI.Container.Services())
        {
        }

        public SaveRating(IServiceProvider services)
        {
            this.services = services;
            ((IUserRepository)this.services.GetService(typeof(IUserRepository))).SetupTable();
            ((IGameDetailsRepository)this.services.GetService(typeof(IGameDetailsRepository))).SetupTable();
            ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).SetupTable();
        }

        public APIGatewayProxyResponse SaveRatingHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            totalNumberOfRatingsChangedBy = 0;
            totalOfRatingsChangedBy = 0;

            var ratingInfo = SaveIntoGameDetailsTable(request.Body, request.Headers["Authorization"]);
            SaveRatingTotalInformation();
            SaveIntoMiscTable(ratingInfo);

            return Response(ratingInfo);
        }

        private string ConvertName(string name)
        {
            name = name.ToLower();
            name = name.Replace(" ", "_");
            return name;
        }

        private void SaveIntoMiscTable(UpdateUserRatingsResponse ratingInfo)
        {
            var pinballGames = ((IMiscRepository)services.GetService(typeof(IMiscRepository)))
                    .Load("Games", "Pinball");
            var pinballNames = pinballGames.List1.ConvertAll(g => ConvertName(g));
            pinballNames.AddRange(pinballGames.List2.ConvertAll(g => ConvertName(g)));

            var pinballRatings = ((IMiscRepository)services.GetService(typeof(IMiscRepository)))
                    .Load("Ratings", "Pinball");
            var arcadeRatings = ((IMiscRepository)services.GetService(typeof(IMiscRepository)))
                    .Load("Ratings", "Arcade Games");

            foreach (RatingInformation rating in ratingInfo.Ratings)
            {
                if (pinballNames.Contains(rating.GameName))
                {
                    if (pinballRatings.Dictionary == null)
                    {
                        pinballRatings.Dictionary = new Dictionary<string, string>();
                    }

                    if (pinballRatings.Dictionary.ContainsKey(rating.GameName))
                    {
                        pinballRatings.Dictionary[rating.GameName] = rating.Average.ToString() + "," + rating.NumberOfRatings.ToString();
                    }
                    else
                    {
                        pinballRatings.Dictionary.Add(rating.GameName, rating.Average.ToString() + "," + rating.NumberOfRatings.ToString());
                    }
                }
                else
                {
                    if (arcadeRatings.Dictionary == null)
                    {
                        arcadeRatings.Dictionary = new Dictionary<string, string>();
                    }

                    if (arcadeRatings.Dictionary.ContainsKey(rating.GameName))
                    {
                        arcadeRatings.Dictionary[rating.GameName] = rating.Average.ToString() + "," + rating.NumberOfRatings.ToString();
                    }
                    else
                    {
                        arcadeRatings.Dictionary.Add(rating.GameName, rating.Average.ToString() + "," + rating.NumberOfRatings.ToString());
                    }
                }
            }

            ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Save(pinballRatings);
            ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Save(arcadeRatings);
        }

        private void SaveRatingTotalInformation()
        {
            var miscRepository = (IMiscRepository)services.GetService(typeof(IMiscRepository));
            var totalNumber = miscRepository.Load("Ratings", "TotalNumber");
            var total = miscRepository.Load("Ratings", "Total");
            var averageRating = miscRepository.Load("Ratings", "Average");

            var totalNumberInt = int.Parse(totalNumber.Value);
            totalNumber.Value = (totalNumberInt + totalNumberOfRatingsChangedBy).ToString();

            var totalInt = int.Parse(total.Value);
            total.Value = (totalInt + totalOfRatingsChangedBy).ToString();

            averageRating.Value = Math.Round(double.Parse(total.Value) / double.Parse(totalNumber.Value), 2).ToString();

            miscRepository.Save(totalNumber);
            miscRepository.Save(total);
            miscRepository.Save(averageRating);
        }

        private UpdateUserRatingsResponse SaveIntoGameDetailsTable(string requestBody, string token)
        {
            var data = JsonConvert.DeserializeObject<SaveRatingInformation>(requestBody);
            ValidateJwt(token, data);
            var gameDetailsRepository = (IGameDetailsRepository)services.GetService(typeof(IGameDetailsRepository));
            var userRepository = (IUserRepository)services.GetService(typeof(IUserRepository));
            var user = userRepository.Load(data.Username);

            var ratinginfo = new List<RatingInformation>();

            foreach (SaveSingleRatingInformationInput input in data.Ratings)
            {
                var ratingFromGameDetailsTable = ((IGameDetailsRepository)services.GetService(typeof(IGameDetailsRepository)))
                    .Load(input.GameName, "Rating");

                if (ratingFromGameDetailsTable == null)
                {
                    ratingFromGameDetailsTable = new GameDetailsRecord();
                    ratingFromGameDetailsTable.Game = input.GameName;
                    ratingFromGameDetailsTable.SortKey = "Rating";
                    ratingFromGameDetailsTable.DataType = "Rating";
                    ratingFromGameDetailsTable.CreatedAt = DateTime.Now;

                    ratingFromGameDetailsTable.NumberOfRatings = 1;
                    ratingFromGameDetailsTable.Average = input.Rating;
                    ratingFromGameDetailsTable.Total = input.Rating;
                    ratingFromGameDetailsTable.CreatedAt = DateTime.Now;
                    ratingFromGameDetailsTable.Ratings = new Dictionary<string, int>
                    {
                        { data.Username, input.Rating },
                    };

                    totalNumberOfRatingsChangedBy++;
                    totalOfRatingsChangedBy += input.Rating;
                }
                else
                {

                    if (ratingFromGameDetailsTable.Ratings.ContainsKey(data.Username))
                    {
                        var oldRating = ratingFromGameDetailsTable.Ratings[data.Username];
                        var newRating = input.Rating;

                        if (oldRating != newRating)
                        {
                            ratingFromGameDetailsTable.Ratings[data.Username] = newRating;
                            ratingFromGameDetailsTable.Total += newRating - oldRating;
                        }

                        totalOfRatingsChangedBy += newRating - oldRating;
                    }
                    else
                    {
                        ratingFromGameDetailsTable.Ratings.Add(data.Username, input.Rating);
                        ratingFromGameDetailsTable.NumberOfRatings++;
                        ratingFromGameDetailsTable.Total += input.Rating;

                        totalNumberOfRatingsChangedBy++;
                        totalOfRatingsChangedBy += input.Rating;
                    }

                    var average = (double)ratingFromGameDetailsTable.Total / ratingFromGameDetailsTable.NumberOfRatings;

                    ratingFromGameDetailsTable.Average = Math.Round(average, 2);
                }

                ((IGameDetailsRepository)services.GetService(typeof(IGameDetailsRepository))).Save(ratingFromGameDetailsTable);
                ratinginfo.Add(new RatingInformation
                {
                    GameName = ratingFromGameDetailsTable.Game,
                    Average = ratingFromGameDetailsTable.Average,
                    CreatedAt = ratingFromGameDetailsTable.CreatedAt,
                    UpdatedAt = ratingFromGameDetailsTable.UpdatedAt,
                    NumberOfRatings = ratingFromGameDetailsTable.NumberOfRatings,
                    Ratings = ratingFromGameDetailsTable.Ratings,
                    Total = ratingFromGameDetailsTable.Total,
                });
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

            var averageOfAllGamesRecord = ((IMiscRepository)this.services.GetService(typeof(IMiscRepository)))
                    .Load("Ratings", "Average");
            var averageOfAllGames = double.Parse(averageOfAllGamesRecord.Value);

            foreach (RatingInformation info in ratingInfo.Ratings)
            {
                var average = info.Average;
                var ratings = info.NumberOfRatings;
                var weightedAverage = WeightedAverageCalculator.CalculateAverage(average, (double)ratings, averageOfAllGames);

                response.Games.Add(info.GameName, new SingleRatingInformationResponse
                {
                    Average = info.Average,
                    NumberOfRatings = info.NumberOfRatings,
                    WeightedAverage = weightedAverage,
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
