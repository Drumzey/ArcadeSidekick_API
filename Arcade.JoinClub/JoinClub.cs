using System;
using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Messages;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.JoinClub
{
    public class JoinClub
    {
        private IServiceProvider services;

        public JoinClub()
            : this(DI.Container.Services())
        {
        }

        public JoinClub(IServiceProvider services)
        {
            this.services = services;
            ((IClubRepository)this.services.GetService(typeof(IClubRepository))).SetupTable();
            ((IUserRepository)this.services.GetService(typeof(IUserRepository))).SetupTable();
            ((IMessageRepository)this.services.GetService(typeof(IMessageRepository))).SetupTable();
        }

        public APIGatewayProxyResponse JoinClubHandler(APIGatewayProxyRequest request, ILambdaContext context)
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

                var userRepository = (IUserRepository)services.GetService(typeof(IUserRepository));
                var user = userRepository.Load(data.Username);

                var clubRepository = (IClubRepository)services.GetService(typeof(IClubRepository));
                var club = clubRepository.Load(data.Clubname);

                var userResult = UpdateUser(data, user);
                var clubResult = UpdateClub(data, club);

                if (userResult && clubResult)
                {
                    clubRepository.Save(club);
                    userRepository.Save(user);
                }
                else
                {
                    if (!clubResult)
                    {
                        return PasswordErrorResponse();
                    }
                    else
                    {
                        return UserDoesNotExistErrorResponse();
                    }
                }

                SendClubJoinMessage(data.Username, club.AdminUsers, club.Name);

                return OkResponse(club);
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message);
            }
        }

        private void SendClubJoinMessage(string userName, List<string> adminUsers, string clubName)
        {
            try
            {
                foreach (string name in adminUsers)
                {
                    Arcade.Shared.Messages.CreateMessage.Create(
                       services,
                       name,
                       "Arcade Sidekick",
                       $"A new user has joined your {clubName} club - {userName}.",
                       Shared.Messages.MessageTypeEnum.JoinedClub,
                       null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to send club joining messages");
            }
        }

        private bool UpdateUser(EditClubMembership data, UserInformation user)
        {
            if (user == null)
            {
                return false;
            }

            if (user.Clubs == null)
            {
                user.Clubs = new List<string>();
            }

            if (!user.Clubs.Contains(data.Clubname))
            {
                user.Clubs.Add(data.Clubname);
            }

            return true;
        }

        private bool UpdateClub(EditClubMembership data, ClubInformation club)
        {
            if (!string.IsNullOrWhiteSpace(club.Secret))
            {
                if (club.Secret != data.Password)
                {
                    return false;
                }
            }

            if (club.Members == null)
            {
                club.Members = new List<string>();
            }

            if (!club.Members.Contains(data.Username))
            {
                club.Members.Add(data.Username);
            }

            return true;
        }

        private APIGatewayProxyResponse OkResponse(ClubInformation club)
        {
            club.Secret = "****";

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(club),
            };
        }

        private APIGatewayProxyResponse UserDoesNotExistErrorResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                Body = "{ \"message\": \"Error. User does not exist\"}",
            };
        }

        private APIGatewayProxyResponse PasswordErrorResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                Body = "{ \"message\": \"Error. Password Incorrect\"}",
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
