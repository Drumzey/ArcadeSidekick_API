using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared
{
    public class SaveRatingInformationInput
    {
        public string Username { get; set; }

        public string GameName { get; set; }

        public int Rating { get; set; }
    }
}
