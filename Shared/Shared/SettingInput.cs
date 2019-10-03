using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared.Shared
{
    public class SettingInput
    {
        public string GameName { get; set; }

        public string LevelName { get; set; }

        public string Difficulty { get; set; }

        public int Lives { get; set; }

        public int ExtraLivesAt { get; set; }

        public string MameOrPCB { get; set; }
    }
}
