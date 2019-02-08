using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared
{
    public interface IEnvironmentVariables
    {
        string UserInformationTableName { get; }

        string RatingInformationTableName { get; }
    }
}
