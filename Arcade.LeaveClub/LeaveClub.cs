using System;
using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.LeaveClub
{
    public class LeaveClub
    {
        private IServiceProvider services;

        public LeaveClub()
            : this(DI.Container.Services())
        {
        }

        public LeaveClub(IServiceProvider services)
        {
            this.services = services;
            ((IClubRepository)this.services.GetService(typeof(IClubRepository))).SetupTable();
            ((IUserRepository)this.services.GetService(typeof(IUserRepository))).SetupTable();
        }

        public APIGatewayProxyResponse LeaveClubHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<EditClubMembership>(request.Body);

                if (string.IsNullOrEmpty(data.Clubname))
                {
                    return ErrorResponse("No clubname given");
                }

                if (string.IsNullOrEmpty(data.Username))
                {
                    return ErrorResponse("No username given");
                }

                UpdateUser(data);
                var club = UpdateClub(data);
                return OkResponse(club);
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        private ClubInformation UpdateClub(EditClubMembership data)
        {
            var clubRepository = (IClubRepository)services.GetService(typeof(IClubRepository));
            var club = clubRepository.Load(data.Clubname);

            if (club.Members == null)
            {
                club.Members = new List<string>();
            }

            if (club.Members.Contains(data.Username))
            {
                club.Members.Remove(data.Username);
            }

            clubRepository.Save(club);

            return club;
        }

        private void UpdateUser(EditClubMembership data)
        {
            var userRepository = (IUserRepository)services.GetService(typeof(IUserRepository));
            var user = userRepository.Load(data.Username);

            if (user.Clubs == null)
            {
                user.Clubs = new List<string>();
            }

            if (user.Clubs.Contains(data.Clubname))
            {
                user.Clubs.Remove(data.Clubname);
            }

            userRepository.Save(user);
        }

        private APIGatewayProxyResponse OkResponse(ClubInformation club)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(club),
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
    }
}
