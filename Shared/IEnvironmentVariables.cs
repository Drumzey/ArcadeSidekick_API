using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared
{
    public interface IEnvironmentVariables
    {
        //Deffo needed
        string GameDetailsTableName { get; }

        string MiscTableName { get; }

        string UserInformationTableName { get; }




        string RatingInformationTableName { get; }

        string ObjectTableName { get; }
        
        string ClubTableName { get; }

        string EmailAddress { get; }

        string EmailPassword { get; }

        string ConsumerAPIKey { get; }

        string ConsumerAPISecretKey { get; }

        string AccessToken { get; }

        string AccessTokenSecret { get; }

        string Categories { get; }

        string TweetsOn { get; }

        string LocationTableName { get; }

        string MessageTableName { get; }
    }
}
