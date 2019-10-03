using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared
{
    public class EnvironmentVariables : IEnvironmentVariables
    {
        public string UserInformationTableName => GetValue("UserInformationTableName");

        public string RatingInformationTableName => GetValue("RatingInformationTableName");

        public string ObjectTableName => GetValue("ObjectTableName");

        public string GameTableName => GetValue("GameTableName");

        public string ClubTableName => GetValue("ClubTableName");

        public string EmailAddress => GetValue("EmailAddress");

        public string EmailPassword => GetValue("EmailPassword");

        public string ConsumerAPIKey => GetValue("ConsumerAPIKey");

        public string ConsumerAPISecretKey => GetValue("ConsumerAPISecretKey");

        public string AccessToken => GetValue("AccessToken");

        public string AccessTokenSecret => GetValue("AccessTokenSecret");

        public string Categories => GetValue("Categories");

        public string TweetsOn => GetValue("TweetsOn");

        public string ListItemsTableName => GetValue("ListItemsTableName");

        public string DetailedScoresByUserNameTableName => GetValue("DetailedScoresByUserNameTableName");

        public string SettingsByGameNameTableName => GetValue("SettingsByGameNameTableName");

        public string ScoreByLocationTableName => GetValue("ScoreByLocationTableName");

        public string LocationByMachineTableName => GetValue("LocationByMachineTableName");

        public string ArcadeMachinesByLocationTableName => GetValue("ArcadeMachinesByLocationTableName");

        public string LocationTableName => GetValue("LocationTableName");

        public string ScoreByGameNameTableName => GetValue("ScoreByGameNameTableName");

        public string ChallengeTableName => GetValue("ChallengeTableName");

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
