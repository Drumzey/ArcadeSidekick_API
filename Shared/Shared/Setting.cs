using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared.Shared
{
    public class Setting
    {
        public string LevelName { get; set; }

        public string Difficulty { get; set; }

        public int Lives { get; set; }

        public int ExtraLivesAt { get; set; }

        public string MameOrPCB { get; set; }
    }
}
