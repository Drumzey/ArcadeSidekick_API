using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared
{
    public interface IEnvironmentVariables
    {
        string UserInformationTableName { get; }

        string RatingInformationTableName { get; }

        string ObjectTableName { get; }

        string GameTableName { get; }

        string ClubTableName { get; }

        string EmailAddress { get; }

        string EmailPassword { get; }

        string ConsumerAPIKey { get; }

        string ConsumerAPISecretKey { get; }

        string AccessToken { get; }

        string AccessTokenSecret { get; }

        string Categories { get; }

        string TweetsOn { get; }

        string ListItemsTableName { get; }

        string DetailedScoresByUserNameTableName { get; }

        string SettingsByGameNameTableName { get; }

        string ScoreByLocationTableName { get; }

        string LocationByMachineTableName { get; }

        string ArcadeMachinesByLocationTableName { get; }

        string LocationTableName { get; }

        string ScoreByGameNameTableName { get; }

        string ChallengeTableName { get; }

        string MessageTableName { get; }
    }
}
