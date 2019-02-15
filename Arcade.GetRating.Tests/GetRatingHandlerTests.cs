using System;
using System.Collections.Generic;

using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Moq;
using Newtonsoft.Json;

namespace Arcade.GetRating.Tests
{
    public class GetRatingHandlerTests
    {
        private const string RatingInput = "{\"GameName\":\"Bubble Bobble\"}";

        [Fact]
        public void GetRatingHandler_WhenCalledWithGameNameThatHasNotBeenSeen_ReturnsNull()
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

            var function = new GetRating(services);
            var context = new TestLambdaContext();
            var result = function.GetRatingHandler(request, context);

            var newRatingInformation = JsonConvert.DeserializeObject<SingleRatingInformationResponse>(result.Body);

            Assert.Equal(0, newRatingInformation.NumberOfRatings);
            Assert.Equal(0, newRatingInformation.Average);

            ratingInfoRepository.Verify(k => k.Load(It.IsAny<String>()), Times.Once());
        }

        [Fact]
        public void GetRatingHandler_WhenCalledWithGameName_GetsRecord()
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

            var function = new GetRating(services);
            var context = new TestLambdaContext();
            var result = function.GetRatingHandler(request, context);

            var newRatingInformation = JsonConvert.DeserializeObject<SingleRatingInformationResponse>(result.Body);

            Assert.Equal(1, newRatingInformation.NumberOfRatings);
            Assert.Equal(2, newRatingInformation.Average);

            ratingInfoRepository.Verify(k => k.Load(It.IsAny<String>()), Times.Once());
        }
    }
}
