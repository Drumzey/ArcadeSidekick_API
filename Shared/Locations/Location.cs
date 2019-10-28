using System.Collections.Generic;

namespace Arcade.Shared.Locations
{
    public class Location
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public string Website { get; set; }

        public string Information { get; set; }

        public string SubmissionRules { get; set; }

        public List<string> GamesAvailable { get; set; }
    }
}
