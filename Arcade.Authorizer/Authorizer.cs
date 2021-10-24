using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Arcade.Shared;
using Arcade.Shared.Repositories;
using Microsoft.IdentityModel.Tokens;
using static Amazon.Lambda.APIGatewayEvents.APIGatewayCustomAuthorizerPolicy;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Arcade.Authorizer
{
    public class Authorizer
    {
        private IServiceProvider services;

        public Authorizer()
            : this(DI.Container.Services())
        {
        }

        public Authorizer(IServiceProvider services)
        {
            this.services = services;
            ((IUserRepository)this.services.GetService(typeof(IUserRepository))).SetupTable();
        }

        public APIGatewayCustomAuthorizerResponse AuthorizerHandler(APIGatewayCustomAuthorizerRequest tokenContext, ILambdaContext context)
        {
            var authorizationToken = tokenContext.AuthorizationToken;
            var methodArn = tokenContext.MethodArn;

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

            if (ValidateToken(authorizationToken, user.Secret))
            {
                var allowedIAMPolicyStatment = GetAllowIAMPolicyStatement(jwt.Issuer);
                return CustomAuthorizerResponse(allowedIAMPolicyStatment, jwt.Issuer);
            }

            var deniedIAMPolicyStatment = GetDeniedIAMPolicyStatement(methodArn, jwt.Issuer);
            return CustomAuthorizerResponse(deniedIAMPolicyStatment, jwt.Issuer);
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
            catch
            {
                Console.WriteLine("Cant validate token");
                throw new Exception("Unauthorized");
            }

            return true;
        }

        private APIGatewayCustomAuthorizerResponse CustomAuthorizerResponse(IAMPolicyStatement iamPolicyStatement, string applicationId)
        {
            var customAuthorizerResponse = new APIGatewayCustomAuthorizerResponse()
            {
                PrincipalID = applicationId,
            };
            customAuthorizerResponse.PolicyDocument.Statement.Add(iamPolicyStatement);
            return customAuthorizerResponse;
        }

        private IAMPolicyStatement GetDeniedIAMPolicyStatement(string methodArn, string applicationId)
        {
            var iamPolicyStatement = new IAMPolicyStatement
            {
                Action = new HashSet<string>() { Constants.PolicyStatementAction },
            };
            var policyStatementResource = methodArn;
            iamPolicyStatement.Effect = Constants.DenyPolicyStatementEffect;
            iamPolicyStatement.Resource = new HashSet<string>() { policyStatementResource };
            return iamPolicyStatement;
        }

        private IAMPolicyStatement GetAllowIAMPolicyStatement(string applicationId)
        {
            var iamPolicyStatement = new IAMPolicyStatement
            {
                Action = new HashSet<string>() { Constants.PolicyStatementAction },
            };
            var policyStatementResource = "*";
            iamPolicyStatement.Resource = new HashSet<string>() { policyStatementResource };
            return iamPolicyStatement;
        }

        private UserInformation GetUser(string username)
        {
            Console.WriteLine($"UserName {username}");
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
