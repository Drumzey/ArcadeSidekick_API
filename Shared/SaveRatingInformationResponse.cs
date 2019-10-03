using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared
{
    public class SaveRatingInformationResponse
    {
        public int NumberOfRatingsForUser { get; set; }

        public Dictionary<string, SingleRatingInformationResponse> Games { get; set; }
    }
}
