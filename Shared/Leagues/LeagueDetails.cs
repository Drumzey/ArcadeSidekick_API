using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace Arcade.Shared.Leagues
{
    public class LeagueDetails
    {
        [DynamoDBHashKey]
        public string Name { get; set; }

        public string ClubName { get; set; }

        public List<string> InvitedUserNames { get; set; }

        public List<string> AcceptedUserNames { get; set; }

        /// <summary>
        /// This dictionary contains the names of the games in the league and the id of
        /// the settings available for that game (i.e 3 lives, pcb, etc.....)
        /// </summary>
        public Dictionary<string, string> GamesToPlay { get; set; }

        public Dictionary<string, int> ScoresForPlace { get; set; }

        public int ScoreForPlaying { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the league may be private, by invite only
        /// I.e Monday night Gamers League
        /// or public
        /// i.e The Arcade Sidkick shmup league.
        /// </summary>
        public bool Private { get; set; }

        /// <summary>
        /// For public leagues teh current standing will be updated on a per day/2day etc basis
        /// and stored here.
        /// </summary>
        public Dictionary<string, int> CurrentStanding { get; set; }
    }
}
