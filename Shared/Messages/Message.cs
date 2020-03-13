using System;
using System.Collections.Generic;

namespace Arcade.Shared.Messages
{
    public enum MessageTypeEnum
    {
        ScoreBeaten,
        ChallengeReceived,
        ClubChallengeReceived,
        General,
        ClubEvent,
        ClubNews,
        ClubInvite,
        JoinedClub,
    }

    public class Message
    {
        public string From { get; set; }

        public string Text { get; set; }

        public MessageTypeEnum MessageType { get; set; }

        public Dictionary<string, string> Data { get; set; }

        public DateTime TimeSet { get; set; }

        public bool Seen { get; set; }
    }
}
