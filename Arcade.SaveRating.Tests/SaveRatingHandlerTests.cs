using System;
using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Arcade.SaveRating.Tests
{
    public class SaveRatingHandlerTests
    {
        private const string RatingInput = "{\"Username\":\"Drumzey\",\"Ratings\":[{\"GameName\":\"Bubble Bobble\",\"Rating\":10},{\"GameName\":\"Amidar\",\"Rating\":9}]}";

        [Fact]
        public void SaveRatingHandler_WhenCalledWithNewRatingInformation_CreatesNewRecord()
        {
            APIGatewayProxyRequest request;
            var headers = new Dictionary<string, string>();
            headers.Add("Authorization", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJEcnVtemV5In0.1sahSegWwFvkxEbV9uyKc2dmNbSSOe-UH5utOyHFMLc");

            request = new APIGatewayProxyRequest
            {
                Headers = headers,
                Body = RatingInput,
            };

            RatingInformation info = null;

            var ratingInfoRepository = new Mock<IRatingRepository>();
            ratingInfoRepository.Setup(x => x.Load("Bubble Bobble")).Returns(info);
            ratingInfoRepository.Setup(x => x.Load("Amidar")).Returns(info);

            var services = DI.Container.Services(null, ratingInfoRepository);

            var function = new SaveRating(services);
            var context = new TestLambdaContext();
            var result = function.SaveRatingHandler(request, context);

            var newRatingInformation = JsonConvert.DeserializeObject<SaveRatingInformationResponse>(result.Body);

            Assert.Equal(1, newRatingInformation.Games["Bubble Bobble"].NumberOfRatings);
            Assert.Equal(10, newRatingInformation.Games["Bubble Bobble"].Average);

            Assert.Equal(1, newRatingInformation.Games["Amidar"].NumberOfRatings);
            Assert.Equal(9, newRatingInformation.Games["Amidar"].Average);

            ratingInfoRepository.Verify(k => k.Save(It.IsAny<RatingInformation>()), Times.Exactly(2));
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

            var newRatingInformation = JsonConvert.DeserializeObject<SingleRatingInformationResponse>(result.Body);

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

            var newRatingInformation = JsonConvert.DeserializeObject<SingleRatingInformationResponse>(result.Body);

            Assert.Equal(1, newRatingInformation.NumberOfRatings);
            Assert.Equal(10, newRatingInformation.Average);

            ratingInfoRepository.Verify(k => k.Save(It.IsAny<RatingInformation>()), Times.Once());
        }
    }
}
