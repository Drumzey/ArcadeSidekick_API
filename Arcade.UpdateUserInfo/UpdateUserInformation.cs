using System.Collections.Generic;

namespace Arcade.UpdateUserInfo
{
    public class UpdateUserInformation
    {
        public string Username { get; set; }

        public string TwitterHandle { get; set; }

        public string Location { get; set; }

        public string DOB { get; set; }

        public string YouTubeChannel { get; set; }

        public List<string> Friends { get; set; }
    }
}
