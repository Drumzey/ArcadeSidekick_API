using System.Collections.Generic;

namespace Arcade.Shared
{
    public class SaveUserInformationInput
    {
        public string Username { get; set; }

        public Dictionary<string, string> Games { get; set; }

        public Dictionary<string, int> Ratings { get; set; }
    }
}
