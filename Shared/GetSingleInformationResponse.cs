using System.Collections.Generic;

namespace Arcade.Shared
{
    public class GetSingleInformationResponse
    {
        public string Username { get; set; }

        public Dictionary<string, string> Games { get; set; }

        public Dictionary<string, int> Ratings { get; set; }

        public List<string> Clubs { get; set; }
    }
}
