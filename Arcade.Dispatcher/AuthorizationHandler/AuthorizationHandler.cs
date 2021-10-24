using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace Arcade.Dispatcher.AuthorizationHandler
{
    public class AuthorizationHandler
    {
        IServiceProvider services;

        public AuthorizationHandler(IServiceProvider services)
        {
            this.services = services;
        }

        public bool Authorize(APIGatewayProxyRequest request)
        {
            Console.WriteLine(request.Headers["Authorization"]);
            var authorizationToken = request.Headers["Authorization"];

            if (IsJwtTokenMalformed(authorizationToken))
            {
                Console.WriteLine("Token is malformed");
                throw new Exception("Unauthorized");
            }

            var jwt = GetJWT(authorizationToken);

            var user = GetUser(jwt.Id);

            if (user == null)
            {
                Console.WriteLine("user is null");
                throw new Exception("No User found");
            }

            Console.WriteLine($"user name is {user.Username}");

            if (!user.Verified)
            {
                throw new Exception("Cannot save data as user not verified");
            }

            try
            {
                if (ValidateToken(authorizationToken, user.Secret))
                {
                    return true;
                }
            }
            catch
            {
                Console.WriteLine("Cant validate token");
                return false;
            }

            return false;
        }


        private bool IsJwtTokenMalformed(string token)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            return !handler.CanReadToken(token);
        }

        private JwtSecurityToken GetJWT(string token)
        {
            JwtSecurityToken jwtToken;
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            try
            {
                jwtToken = handler.ReadJwtToken(token);
            }
            catch
            {
                Console.WriteLine("Cant read token");
                throw new Exception("Unauthorized");
            }

            if ((jwtToken == null) || (jwtToken.Payload.Count < 1))
            {
                Console.WriteLine("No payload");
                throw new Exception("No Payload");
            }

            return jwtToken;
        }

        private bool ValidateToken(string token, string secret)
        {
            Console.WriteLine($"token {token})");
            Console.WriteLine($"secret {secret}");

            TokenValidationParameters validationParameters = null;

            validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidIssuer = "Sidekick",
            };

            try
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Cant validate token");
                throw new Exception("Unauthorized");
            }

            return true;
        }
        
        private UserInformation GetUser(string username)
        {
            Console.WriteLine($"UserName {username}");
            ((IUserRepository)services.GetService(typeof(IUserRepository))).SetupTable();
            var user = ((IUserRepository)services.GetService(typeof(IUserRepository))).Load(username);

            if (user == null)
            {
                Console.WriteLine("Cant find user");
                throw new Exception("Unauthorized");
            }

            return user;
        }
    }
}
