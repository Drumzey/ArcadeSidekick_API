using System;
using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Misc;
using Arcade.Shared.Repositories;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.UpdateUserInfo
{
    public class UpdateUserInfo
    {
        private IServiceProvider services;
        private List<string> oldFriends;

        public UpdateUserInfo()
            : this(DI.Container.Services())
        {
        }

        public UpdateUserInfo(IServiceProvider services)
        {
            this.services = services;
            ((IUserRepository)this.services.GetService(typeof(IUserRepository))).SetupTable();
            ((IMiscRepository)this.services.GetService(typeof(IMiscRepository))).SetupTable();
        }

        public APIGatewayProxyResponse UpdateUserInfoHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                Console.WriteLine(request.Body);

                var userInfo = JsonConvert.DeserializeObject<UpdateUserInformation>(request.Body);

                var user = ((IUserRepository)services.GetService(typeof(IUserRepository))).Load(userInfo.Username);

                Console.WriteLine(user);

                if (user == null)
                {
                    return ErrorResponse("User record not found.", HttpStatusCode.NotFound);
                }

                Console.WriteLine("Updating user in DB");
                Console.WriteLine(user.Username);
                UpdateUserInDatabase(userInfo, user);

                Console.WriteLine("Updating twitter handle");
                CreateUserTwitterHandleInMiscTable(userInfo);

                Console.WriteLine("updating friends");
                UpdateFriendsListForMessaging(userInfo);

                return OkResponse();
            }
            catch (Exception e)
            {
                return ErrorResponse(e.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// When a user uploads their friends, we can use this information to construct
        /// a list of who is following who
        /// that way when a score is made, additional messages can be sent
        /// </summary>
        /// <param name="userInfo"></param>
        private void UpdateFriendsListForMessaging(UpdateUserInformation userInfo)
        {
            if (userInfo.Friends == null)
                return;

            foreach (string friend in userInfo.Friends)
            {
                if (friend == userInfo.Username)
                    continue;

                var peopleWhoFollowX = ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Load("Followers", friend);

                if (peopleWhoFollowX == null)
                {
                    peopleWhoFollowX = new Misc();
                    peopleWhoFollowX.Key = "Followers";
                    peopleWhoFollowX.SortKey = friend;
                    peopleWhoFollowX.List1 = new List<string>();
                    peopleWhoFollowX.List1.Add(userInfo.Username);
                }
                else
                {
                    if(!peopleWhoFollowX.List1.Contains(userInfo.Username))
                    {
                        peopleWhoFollowX.List1.Add(userInfo.Username);
                    }
                }

                ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Save(peopleWhoFollowX);
            }            

            //Unfriending
            foreach(string oldFriend in oldFriends)
            {
                if (oldFriend == userInfo.Username)
                    continue;

                //If friend was present in old friends and not in new friends then we need to remove them
                if (!userInfo.Friends.Contains(oldFriend))
                {
                    var peopleWhoFollowX = ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Load("Followers", oldFriend);
                    peopleWhoFollowX.List1.Remove(userInfo.Username);
                    ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Save(peopleWhoFollowX);
                }
            }
        }

        private void CreateUserTwitterHandleInMiscTable(UpdateUserInformation userInfo)
        {
            try
            {
                var tweeties = ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Load("Twitter", "Users");

                if (tweeties == null)
                {
                    tweeties = new Misc();
                    tweeties.Key = "Twitter";
                    tweeties.SortKey = "Users";
                    tweeties.Dictionary = new Dictionary<string, string>();
                }

                if (!tweeties.Dictionary.ContainsKey(userInfo.Username))
                {
                    tweeties.Dictionary.Add(userInfo.Username, userInfo.TwitterHandle);
                }
                else
                {
                    tweeties.Dictionary[userInfo.Username] = userInfo.TwitterHandle;
                }

                ((IMiscRepository)services.GetService(typeof(IMiscRepository))).Save(tweeties);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        
        private void UpdateUserInDatabase(UpdateUserInformation userInfo, UserInformation user)
        {
            oldFriends = new List<string>();

            if (user.Friends != null)
            {
                oldFriends = new List<string>(user.Friends);
            }

            user.Friends = userInfo.Friends;
            user.TwitterHandle = userInfo.TwitterHandle;

            Console.WriteLine("About To Save");
            Console.WriteLine(user);

            ((IUserRepository)services.GetService(typeof(IUserRepository))).Save(user);            
        }

        private APIGatewayProxyResponse OkResponse()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "{ \"message\": \"User information updated correctly\"}",
            };
        }

        private APIGatewayProxyResponse ErrorResponse(string error, HttpStatusCode code)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)code,
                Body = "{ \"message\": \"Error. " + error + "\"}",
            };
        }
    }
}
