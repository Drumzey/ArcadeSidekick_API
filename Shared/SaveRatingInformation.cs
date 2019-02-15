using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared
{
    public class SaveRatingInformation
    {
        public string Username { get; set; }
        public List<SaveSingleRatingInformationInput> Ratings { get; set; }
    }
}
