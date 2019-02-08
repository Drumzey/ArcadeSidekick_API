using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared
{
    public class EnvironmentVariables : IEnvironmentVariables
    {
        public string UserInformationTableName => GetValue("UserInformationTableName");

        public string RatingInformationTableName => GetValue("RatingInformationTableName");

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
