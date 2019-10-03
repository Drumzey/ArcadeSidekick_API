using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared
{
    public class IndividualChallenge
    {
        public string From { get; set; }

        public string GameName { get; set; }

        public string Message { get; set; }

        public DateTime Expires { get; set; }

        public bool Seen { get; set; }
    }
}
