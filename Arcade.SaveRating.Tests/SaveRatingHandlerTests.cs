using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using System.Collections.Generic;
using Arcade.Shared.Repositories;
using Moq;
using Arcade.Shared;
using Newtonsoft.Json;
using System;

namespace Arcade.SaveRating.Tests
{
    public class SaveRatingHandlerTests
    {
        private const string RatingInput = "{\"UserName\":\"Drumzey\",\"GameName\":\"Bubble Bobble\",\"Rating\":10}";

        [Fact]
        public void SaveRatingHandler_WhenCalledWithNewRatingInformation_CreatesNewRecord()
        {
            APIGatewayProxyRequest request;
            var headers = new Dictionary<string, string>()
            {
            };
            request = new APIGatewayProxyRequest
            {
                Body = RatingInput,
            };

            RatingInformation info = null;

            var ratingInfoRepository = new Mock<IRatingRepository>();
            ratingInfoRepository.Setup(x => x.Load(It.IsAny<string>())).Returns(info);

            var services = DI.Container.Services(null, ratingInfoRepository);

            var function = new SaveRating(services);
            var context = new TestLambdaContext();
            var result = function.SaveRatingHandler(request, context);

            var newRatingInformation = JsonConvert.DeserializeObject<RatingInformationResponse>(result.Body);

            Assert.Equal(1, newRatingInformation.NumberOfRatings);
            Assert.Equal(10, newRatingInformation.Average);

            ratingInfoRepository.Verify(k => k.Save(It.IsAny<RatingInformation>()), Times.Once());
        }

        [Fact]
        public void SaveRatingHandler_WhenCalledWithExistingRatingInformationForNewUser_UpdatesRecord()
        {
            APIGatewayProxyRequest request;
            var headers = new Dictionary<string, string>()
            {
            };
            request = new APIGatewayProxyRequest
            {
                Body = RatingInput,
            };

            RatingInformation info = new RatingInformation
            {
                Average = 2,
                Total = 2,
                GameName = "Bubble Bobble",
                NumberOfRatings = 1,
                Ratings = new Dictionary<string, int>
                {
                    { "Gustree", 2 },
                },
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };            

            var ratingInfoRepository = new Mock<IRatingRepository>();
            ratingInfoRepository.Setup(x => x.Load(It.IsAny<string>())).Returns(info);

            var services = DI.Container.Services(null, ratingInfoRepository);

            var function = new SaveRating(services);
            var context = new TestLambdaContext();
            var result = function.SaveRatingHandler(request, context);

            var newRatingInformation = JsonConvert.DeserializeObject<RatingInformationResponse>(result.Body);

            Assert.Equal(2, newRatingInformation.NumberOfRatings);
            Assert.Equal(6, newRatingInformation.Average);

            ratingInfoRepository.Verify(k => k.Save(It.IsAny<RatingInformation>()), Times.Once());
        }

        [Fact]
        public void SaveRatingHandler_WhenCalledWithExistingRatingInformationForExistingUser_UpdatesRecord()
        {
            APIGatewayProxyRequest request;
            var headers = new Dictionary<string, string>()
            {
            };
            request = new APIGatewayProxyRequest
            {
                Body = RatingInput,
            };

            RatingInformation info = new RatingInformation
            {
                Average = 2,
                Total = 2,
                GameName = "Bubble Bobble",
                NumberOfRatings = 1,
                Ratings = new Dictionary<string, int>
                {
                    { "Drumzey", 2 },
                },
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            var ratingInfoRepository = new Mock<IRatingRepository>();
            ratingInfoRepository.Setup(x => x.Load(It.IsAny<string>())).Returns(info);

            var services = DI.Container.Services(null, ratingInfoRepository);

            var function = new SaveRating(services);
            var context = new TestLambdaContext();
            var result = function.SaveRatingHandler(request, context);

            var newRatingInformation = JsonConvert.DeserializeObject<RatingInformationResponse>(result.Body);

            Assert.Equal(1, newRatingInformation.NumberOfRatings);
            Assert.Equal(10, newRatingInformation.Average);

            ratingInfoRepository.Verify(k => k.Save(It.IsAny<RatingInformation>()), Times.Once());
        }
    }
}
