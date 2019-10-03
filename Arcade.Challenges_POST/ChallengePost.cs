using System;

namespace Arcade.Challenges_POST
{
    internal class ChallengePost
    {
        public string From { get; set; }
        public string To { get; set; }
        public bool ToClubMembers { get; set; }
        public string GameName { get; set; }
        public string Message { get; set; }        
        public DateTime Expires { get; set; }
    }
}
