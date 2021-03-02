using System.Collections.Generic;

namespace Arcade.Shared
{
    public class SaveRatingInformation
    {
        public string Username { get; set; }

        public List<SaveSingleRatingInformationInput> Ratings { get; set; }
    }
}
