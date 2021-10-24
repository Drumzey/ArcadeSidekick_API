using System.Collections.Generic;

namespace Arcade.Shared.Locations
{
    public class Location
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public string Website { get; set; }

        public string Twitter { get; set; }

        public string Facebook { get; set; }

        public string Instagram { get; set; }

        public string Information { get; set; }

        public string SubmissionRules { get; set; }

        public List<string> GamesAvailable { get; set; }

        public bool Private { get; set; }

        public string Password { get; set; }

        public string Country { get; set; }
    }
}
