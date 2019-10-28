using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared
{
    public class EnvironmentVariables : IEnvironmentVariables
    {
        //DEFFO NEEDED
        public string GameDetailsTableName => GetValue("GameDetailsTableName");

        public string MiscTableName => GetValue("MiscTableName");

        public string UserInformationTableName => GetValue("UserInformationTableName");




        public string RatingInformationTableName => GetValue("RatingInformationTableName");

        public string ObjectTableName => GetValue("ObjectTableName");
        
        public string ClubTableName => GetValue("ClubTableName");

        public string EmailAddress => GetValue("EmailAddress");

        public string EmailPassword => GetValue("EmailPassword");

        public string ConsumerAPIKey => GetValue("ConsumerAPIKey");

        public string ConsumerAPISecretKey => GetValue("ConsumerAPISecretKey");

        public string AccessToken => GetValue("AccessToken");

        public string AccessTokenSecret => GetValue("AccessTokenSecret");

        public string Categories => GetValue("Categories");

        public string TweetsOn => GetValue("TweetsOn");

        public string LocationTableName => GetValue("LocationTableName");

        public string MessageTableName => GetValue("MessageTableName");

        private string GetValue(string variableName)
        {
            var value = Environment.GetEnvironmentVariable(variableName);

            if (value == null)
            {
                return string.Empty;
            }

            return value;
        }
    }
}
