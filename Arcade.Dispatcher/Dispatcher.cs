using System;
using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.Dispatcher
{
    public class Dispatcher
    {
        private IServiceProvider services;
        private APIGatewayProxyRequest request;
        private ILambdaContext context;

        public Dispatcher()
            : this(DI.Container.Services())
        {
        }

        public Dispatcher(IServiceProvider services)
        {
            this.services = services;
        }

        public APIGatewayProxyResponse DispatcherHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            this.request = request;
            this.context = context;

            if (request.Resource.StartsWith("/app/clubs") ||
                request.Resource.StartsWith("/app/games") ||
                request.Resource.StartsWith("/website/games") ||
                request.Resource.StartsWith("/app/system") ||
                request.Resource.StartsWith("/app/venues") ||
                request.Resource.StartsWith("/website/venues") ||
                request.Resource.StartsWith("/app/users/messages") ||
                request.Resource.StartsWith("/app/users/profile") ||
                request.Resource.StartsWith("/app/users/update"))
            {
                if (request.HttpMethod == "POST" || request.HttpMethod == "DELETE")
                {
                    Console.WriteLine($"Authorizing {request.HttpMethod} Request");
                    var auth = new AuthorizationHandler.AuthorizationHandler(this.services);
                    var result = auth.Authorize(request);
                    if (!result)
                    {
                        var unAuthResponse = new APIGatewayProxyResponse
                        {
                            StatusCode = (int)HttpStatusCode.Unauthorized,
                            Body = "{ \"message\": \"Error. Unknown end point\"}",
                        };

                        return ErrorResponse(unAuthResponse);
                    }
                    Console.WriteLine("Authorized");
                }
            }

            APIGatewayProxyResponse response;

            if (request.Resource.StartsWith("/app/clubs"))
            {
                response = HandleClubRequest();
            }
            else if (request.Resource.StartsWith("/app/games") ||
                     request.Resource.StartsWith("/website/games"))
            {
                response = HandleGameRequest();
            }
            else if (request.Resource.StartsWith("/app/system"))
            {
                response = HandleSystemRequest();
            }
            else if (request.Resource.StartsWith("/app/users"))
            {
                response = HandleUserRequest();
            }
            else if (request.Resource.StartsWith("/app/venues") ||
                     request.Resource.StartsWith("/website/venues"))
            {
                response = HandleVenueRequest();
            }
            else
            {
                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = "{ \"message\": \"Error. Unknown end point\"}",
                };
            }

            if (response.StatusCode != 200)
            {
                return ErrorResponse(response);
            }

            return OkResponse(response);
        }
        
        private APIGatewayProxyResponse OkResponse(APIGatewayProxyResponse response)
        {
            var headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
            };

            return new APIGatewayProxyResponse
            {
                StatusCode = response.StatusCode,
                Body = response.Body,
                Headers = headers,
            };
        }

        private APIGatewayProxyResponse ErrorResponse(APIGatewayProxyResponse response)
        {
            var headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
            };

            return new APIGatewayProxyResponse
            {
                StatusCode = response.StatusCode,
                Body = response.Body,
                Headers = headers,
            };
        }

        private APIGatewayProxyResponse HandleVenueRequest()
        {
            return VenueHandler.VenuesHandler.HandleRequest(request, context, services);
        }
        
        private APIGatewayProxyResponse HandleClubRequest()
        {
            return ClubHandler.ClubHandler.HandleRequest(request, context, services);
        }

        private APIGatewayProxyResponse HandleUserRequest()
        {
            return UserHandler.UserHandler.HandleRequest(request, context, services);
        }

        private APIGatewayProxyResponse HandleSystemRequest()
        {
            return SystemHandler.SystemHandler.HandleRequest(request, context, services);
        }

        private APIGatewayProxyResponse HandleGameRequest()
        {
            try
            {
                return GameHandler.GameHandler.HandleRequest(request, context, services);
            }
            catch(Exception e)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = "{ \"message\": \"Error. " + e.Message + "\"}",
                };
            }
        }
    }
}
