using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcade.GameDetails.Handlers
{
    public class SettingsHandler
    {
        public List<Setting> Get(GameDetailsRecord details)
        {
            var settings = new List<Setting>();
            settings.AddRange(details.Settings);
            var uniqueSettings = settings.Distinct();
            return uniqueSettings.ToList();
        }

        public List<Setting> Get(IGameDetailsRepository repo, string gameName)
        {
            var details = repo.Load(gameName, "Settings");
            if (details == null)
            {
                return new List<Setting>();
            }

            return Get(details);
        }
    }
}
